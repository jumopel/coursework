using System;
using System.Collections.Generic;
using System.Text;

namespace coursework.DTO
{
    public class AlertMessage
    {
        public string Icon { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string BackgroundColor { get; set; } = string.Empty;
        public string BorderColor { get; set; } = string.Empty;
    }
}
