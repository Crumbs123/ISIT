import tkinter as tk
from tkinter import ttk, messagebox
from datetime import datetime
import requests
import threading

# === БАЗА ЗНАНИЙ (реляционная модель) ===
class WeatherDatabase:
    def __init__(self):
        # Координаты городов для API
        self.cities = {
            "Макеевка": {"lat": 48.0478, "lon": 37.9667},
            "Донецк": {"lat": 48.0159, "lon": 37.8028},
            "Ростов-на-Дону": {"lat": 47.2225, "lon": 39.7183},
            "Москва": {"lat": 55.7558, "lon": 37.6173},
            "Санкт-Петербург": {"lat": 59.9311, "lon": 30.3609}
        }
        
        # Кэш для хранения полученных данных
        self.cache = {}
    
    def get_city_coords(self, city):
        """Получение координат города"""
        return self.cities.get(city)
    
    def cache_data(self, city, data):
        """Кэширование данных"""
        self.cache[city] = {
            "data": data,
            "timestamp": datetime.now()
        }
    
    def get_cached(self, city):
        """Получение кэшированных данных (если не старше 10 минут)"""
        if city in self.cache:
            age = (datetime.now() - self.cache[city]["timestamp"]).seconds
            if age < 600:  # 10 минут
                return self.cache[city]["data"]
        return None

# === API КЛИЕНТ ===
class WeatherAPI:
    def __init__(self):
        self.base_url = "https://api.open-meteo.com/v1/forecast"
    
    def fetch_weather(self, lat, lon):
        """Запрос к Open-Meteo API"""
        params = {
            "latitude": lat,
            "longitude": lon,
            "current": "temperature_2m,relative_humidity_2m,apparent_temperature,"
                      "precipitation,weather_code,surface_pressure,wind_speed_10m",
            "timezone": "auto"
        }
        
        try:
            response = requests.get(self.base_url, params=params, timeout=10)
            response.raise_for_status()
            return response.json()
        except requests.exceptions.RequestException as e:
            raise Exception(f"Ошибка сети: {str(e)}")
    
    @staticmethod
    def decode_weather_code(code):
        """Декодирование кода погоды WMO"""
        codes = {
            0: "Ясно",
            1: "Преимущественно ясно", 2: "Переменная облачность", 3: "Пасмурно",
            45: "Туман", 48: "Изморозь",
            51: "Слабая морось", 53: "Умеренная морось", 55: "Сильная морось",
            61: "Слабый дождь", 63: "Умеренный дождь", 65: "Сильный дождь",
            71: "Слабый снег", 73: "Умеренный снег", 75: "Сильный снег",
            77: "Снежные зерна",
            80: "Слабый ливень", 81: "Умеренный ливень", 82: "Сильный ливень",
            85: "Снегопад", 86: "Сильный снегопад",
            95: "Гроза", 96: "Гроза с градом", 99: "Гроза с сильным градом"
        }
        return codes.get(code, "Неизвестно")

# === МАШИНА ВЫВОДА (правила логического вывода) ===
class InferenceEngine:
    @staticmethod
    def evaluate_temperature(temp):
        """Правило вывода для температуры"""
        if temp < -10:
            return "Очень холодно"
        elif -10 <= temp < 0:
            return "Холодно"
        elif 0 <= temp < 15:
            return "Прохладно"
        elif 15 <= temp < 25:
            return "Тепло"
        else:
            return "Жарко"
    
    @staticmethod
    def evaluate_wind(wind):
        """Правило вывода для ветра"""
        if wind < 5:
            return "Слабый"
        elif 5 <= wind < 15:
            return "Умеренный"
        elif 15 <= wind < 30:
            return "Сильный"
        else:
            return "Ураганный"
    
    @staticmethod
    def evaluate_precipitation(precip):
        """Правило вывода для осадков"""
        if precip == 0:
            return "Нет"
        elif precip <= 2:
            return "Слабые"
        elif precip <= 10:
            return "Умеренные"
        else:
            return "Сильные"
    
    @staticmethod
    def evaluate_humidity(humidity):
        """Правило для влажности"""
        if humidity < 30:
            return "Низкая"
        elif 30 <= humidity <= 60:
            return "Нормальная"
        else:
            return "Высокая"
    
    @staticmethod
    def evaluate_pressure(pressure):
        """Правило для давления"""
        if pressure < 980:
            return "Очень низкое"
        elif 980 <= pressure < 1000:
            return "Низкое"
        elif 1000 <= pressure <= 1025:
            return "Нормальное"
        else:
            return "Высокое"

# === ГРАФИЧЕСКИЙ ИНТЕРФЕЙС ===
class WeatherExpertSystem:
    def __init__(self, root):
        self.root = root
        self.root.title("Экспертная система погоды")
        self.root.geometry("750x550")
        self.root.configure(bg="#f0f4f8")
        
        self.db = WeatherDatabase()
        self.api = WeatherAPI()
        self.engine = InferenceEngine()
        
        self.setup_ui()
        
        # Загрузка данных при запуске
        self.update_weather()
    
    def setup_ui(self):
        # Шапка
        header_frame = tk.Frame(self.root, bg="#1e3a5f", height=80)
        header_frame.pack(fill="x")
        header_frame.pack_propagate(False)
        
        title_label = tk.Label(
            header_frame, 
            text="Экспертная система погоды", 
            font=("Arial", 20, "bold"),
            fg="white",
            bg="#1e3a5f"
        )
        title_label.pack(pady=(15, 0))
        
        subtitle_label = tk.Label(
            header_frame,
            text="Машина вывода реляционного типа | Данные: Open-Meteo",
            font=("Arial", 9),
            fg="#b0c4de",
            bg="#1e3a5f"
        )
        subtitle_label.pack()
        
        # Панель управления
        control_frame = tk.Frame(self.root, bg="#f0f4f8")
        control_frame.pack(pady=20)
        
        tk.Label(
            control_frame,
            text="Выберите город:",
            font=("Arial", 11),
            bg="#f0f4f8"
        ).pack(side="left", padx=5)
        
        self.city_var = tk.StringVar()
        self.city_combo = ttk.Combobox(
            control_frame,
            textvariable=self.city_var,
            values=list(self.db.cities.keys()),
            state="readonly",
            width=20,
            font=("Arial", 10)
        )
        self.city_combo.set("Макеевка")
        self.city_combo.pack(side="left", padx=5)
        self.city_combo.bind("<<ComboboxSelected>>", lambda e: self.update_weather())
        
        self.update_btn = tk.Button(
            control_frame,
            text="🔄 Обновить",
            command=self.update_weather,
            bg="#2c5282",
            fg="white",
            font=("Arial", 10, "bold"),
            padx=20,
            pady=5,
            cursor="hand2"
        )
        self.update_btn.pack(side="left", padx=10)
        
        # Индикатор загрузки
        self.loading_label = tk.Label(
            control_frame,
            text="",
            font=("Arial", 10),
            bg="#f0f4f8",
            fg="#2c5282"
        )
        self.loading_label.pack(side="left", padx=10)
        
        # Основная панель с данными
        self.data_frame = tk.Frame(self.root, bg="#f0f4f8")
        self.data_frame.pack(pady=10, padx=30, fill="both", expand=True)
        
        # Статус бар
        self.status_bar = tk.Label(
            self.root,
            text="Готово",
            font=("Arial", 9),
            bg="#e2e8f0",
            fg="#666",
            anchor="w",
            padx=10
        )
        self.status_bar.pack(fill="x", side="bottom")
    
    def set_loading(self, loading):
        """Переключение состояния загрузки"""
        if loading:
            self.loading_label.config(text="⏳ Загрузка...")
            self.update_btn.config(state="disabled")
            self.city_combo.config(state="disabled")
        else:
            self.loading_label.config(text="")
            self.update_btn.config(state="normal")
            self.city_combo.config(state="readonly")
    
    def update_weather(self):
        """Обновление данных в отдельном потоке"""
        city = self.city_var.get()
        if not city:
            return
        
        # Запускаем в отдельном потоке, чтобы не блокировать GUI
        thread = threading.Thread(target=self._fetch_and_display, args=(city,))
        thread.daemon = True
        thread.start()
    
    def _fetch_and_display(self, city):
        """Получение и отображение данных (в фоновом потоке)"""
        self.root.after(0, self.set_loading, True)
        
        try:
            # Проверяем кэш
            cached = self.db.get_cached(city)
            if cached:
                data = cached
                source = "кэш"
            else:
                # Получаем координаты
                coords = self.db.get_city_coords(city)
                if not coords:
                    raise Exception("Город не найден в базе")
                
                # Запрос к API
                api_data = self.api.fetch_weather(coords["lat"], coords["lon"])
                current = api_data["current"]
                
                # Формируем структуру данных
                data = {
                    "temperature": round(current["temperature_2m"]),
                    "feels_like": round(current["apparent_temperature"]),
                    "humidity": current["relative_humidity_2m"],
                    "wind": round(current["wind_speed_10m"]),
                    "pressure": round(current["surface_pressure"]),
                    "precipitation": current["precipitation"],
                    "condition": self.api.decode_weather_code(current["weather_code"])
                }
                
                # Кэшируем
                self.db.cache_data(city, data)
                source = "API"
            
            # Отображаем в главном потоке
            self.root.after(0, lambda: self._display_data(city, data, source))
            
        except Exception as e:
            self.root.after(0, lambda: self._show_error(str(e)))
    
    def _display_data(self, city, data, source):
        """Отображение данных на экране"""
        self.set_loading(False)
        
        # Применение правил вывода
        temp_eval = self.engine.evaluate_temperature(data["temperature"])
        wind_eval = self.engine.evaluate_wind(data["wind"])
        precip_eval = self.engine.evaluate_precipitation(data["precipitation"])
        humidity_eval = self.engine.evaluate_humidity(data["humidity"])
        pressure_eval = self.engine.evaluate_pressure(data["pressure"])
        
        # Очистка
        for widget in self.data_frame.winfo_children():
            widget.destroy()
        
        # Заголовок города
        city_header = tk.Frame(self.data_frame, bg="#f0f4f8")
        city_header.pack(fill="x", pady=10)
        
        tk.Label(
            city_header,
            text=city,
            font=("Arial", 24, "bold"),
            bg="#f0f4f8",
            fg="#1e3a5f"
        ).pack(side="left")
        
        condition_label = tk.Label(
            city_header,
            text=data["condition"],
            font=("Arial", 12),
            bg="#f0f4f8",
            fg="#666"
        )
        condition_label.pack(side="left", padx=10, pady=8)
        
        # Иконка погоды
        icon_canvas = tk.Canvas(city_header, width=50, height=50, 
                               bg="#f0f4f8", highlightthickness=0)
        icon_canvas.pack(side="right", padx=20)
        self._draw_weather_icon(icon_canvas, data["condition"])
        
        # Карточки с данными
        cards_frame = tk.Frame(self.data_frame, bg="#f0f4f8")
        cards_frame.pack(fill="both", expand=True)
        
        # Первая строка
        row1 = tk.Frame(cards_frame, bg="#f0f4f8")
        row1.pack(fill="x")
        
        self.create_card(row1, "Температура", 
                        f"{data['temperature']}°C", temp_eval, "#ffcccc")
        self.create_card(row1, "Ощущается как", 
                        f"{data['feels_like']}°C", temp_eval, "#cce5ff")
        self.create_card(row1, "Влажность", 
                        f"{data['humidity']}%", humidity_eval, "#ccffcc")
        
        # Вторая строка
        row2 = tk.Frame(cards_frame, bg="#f0f4f8")
        row2.pack(fill="x")
        
        self.create_card(row2, "Ветер", 
                        f"{data['wind']} км/ч", wind_eval, "#ffffcc")
        self.create_card(row2, "Давление", 
                        f"{data['pressure']} гПа", pressure_eval, "#e5ccff")
        self.create_card(row2, "Осадки", 
                        f"{data['precipitation']} мм", precip_eval, "#ccffff")
        
        # Время обновления
        update_time = datetime.now().strftime("%H:%M:%S")
        update_label = tk.Label(
            self.data_frame,
            text=f"Данные для {city} обновлены: {update_time} (источник: {source})",
            font=("Arial", 9),
            bg="#f0f4f8",
            fg="#2e7d32"
        )
        update_label.pack(pady=10)
        
        # Статус бар
        self.status_bar.config(
            text=f"Последнее обновление: {update_time} | Источник: Open-Meteo API ({source})"
        )
    
    def _draw_weather_icon(self, canvas, condition):
        """Рисование иконки погоды"""
        condition_lower = condition.lower()
        
        if "ясно" in condition_lower or "солнечно" in condition_lower:
            # Солнце
            canvas.create_oval(10, 10, 40, 40, fill="#FFD700", outline="#FFA500", width=2)
            for angle in range(0, 360, 45):
                x1 = 25 + 18 * __import__('math').cos(__import__('math').radians(angle))
                y1 = 25 + 18 * __import__('math').sin(__import__('math').radians(angle))
                x2 = 25 + 23 * __import__('math').cos(__import__('math').radians(angle))
                y2 = 25 + 23 * __import__('math').sin(__import__('math').radians(angle))
                canvas.create_line(x1, y1, x2, y2, fill="#FFA500", width=2)
        
        elif "дождь" in condition_lower or "ливень" in condition_lower:
            # Дождь
            canvas.create_oval(10, 5, 35, 25, fill="#808080", outline="#606060")
            for i, x in enumerate([15, 25, 20]):
                canvas.create_line(x, 28, x-3, 40, fill="#4169E1", width=2)
        
        elif "снег" in condition_lower:
            # Снег
            canvas.create_oval(10, 5, 35, 25, fill="#D3D3D3", outline="#A9A9A9")
            for x, y in [(12, 32), (22, 38), (30, 30)]:
                canvas.create_text(x, y, text="❄", font=("Arial", 8), fill="#87CEEB")
        
        elif "облачно" in condition_lower or "пасмурно" in condition_lower:
            # Облако
            canvas.create_oval(5, 15, 30, 35, fill="#D3D3D3", outline="#A9A9A9")
            canvas.create_oval(20, 10, 45, 30, fill="#D3D3D3", outline="#A9A9A9")
            canvas.create_oval(12, 8, 37, 28, fill="#E8E8E8", outline="#D3D3D3")
        
        elif "гроза" in condition_lower:
            # Гроза
            canvas.create_oval(10, 5, 35, 25, fill="#4B4B4B", outline="#2F2F2F")
            canvas.create_polygon(20, 25, 15, 35, 22, 35, 18, 45, 28, 30, 20, 30, 
                                fill="#FFD700", outline="#FFA500")
        
        else:
            # По умолчанию - солнце за облаком
            canvas.create_oval(5, 10, 30, 35, fill="#FFD700", outline="#FFA500")
            canvas.create_oval(15, 15, 45, 40, fill="#E8E8E8", outline="#D3D3D3")
    
    def create_card(self, parent, title, value, subtitle, color):
        """Создание карточки параметра"""
        card = tk.Frame(
            parent,
            bg="white",
            highlightbackground=color,
            highlightthickness=2,
            bd=0
        )
        card.pack(side="left", padx=10, pady=10, expand=True, fill="both")
        
        tk.Label(
            card,
            text=title,
            font=("Arial", 10),
            bg="white",
            fg="#666"
        ).pack(pady=(10, 0))
        
        tk.Label(
            card,
            text=value,
            font=("Arial", 18, "bold"),
            bg="white",
            fg="#1e3a5f"
        ).pack()
        
        tk.Label(
            card,
            text=subtitle,
            font=("Arial", 9),
            bg="white",
            fg="#888"
        ).pack(pady=(0, 10))
        
        return card
    
    def _show_error(self, message):
        """Показ ошибки"""
        self.set_loading(False)
        messagebox.showerror("Ошибка", f"Не удалось получить данные:\n{message}")
        self.status_bar.config(text=f"Ошибка: {message}", fg="red")

# === ЗАПУСК ===
if __name__ == "__main__":
    root = tk.Tk()
    app = WeatherExpertSystem(root)
    root.mainloop()