﻿namespace OtpProvider.Application.DTOs
{
    public class GmailSetting
    {
        public required string FromAddress { get; set; }
        public required string FromName { get; set; }
        public required string AppPassword { get; set; }
    }
}
