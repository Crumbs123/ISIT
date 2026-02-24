// DecisionNode.cs - Класс для узлов дерева решений
using System.Collections.Generic;

namespace DecisionTreeApp
{
    public class DecisionNode
    {
        public string Question { get; set; }
        public string Result { get; set; }
        public DecisionNode YesNode { get; set; }
        public DecisionNode NoNode { get; set; }
        public DecisionNode Parent { get; set; }
        public bool IsResult => !string.IsNullOrEmpty(Result);

        // Для отображения в TreeView
        public string DisplayText => IsResult ? Result : Question;
    }
}