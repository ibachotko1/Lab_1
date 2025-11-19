using System.Collections.Generic;

namespace Warehouse.Back.Models
{
    public class OperationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<PostConditionInfo> PostConditions { get; set; }

        public OperationResult()
        {
            PostConditions = new List<PostConditionInfo>();
        }
    }

    public class PostConditionInfo
    {
        public string Description { get; set; }
        public bool IsSatisfied { get; set; }
        public string Details { get; set; }
    }
}