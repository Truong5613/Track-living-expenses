﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnLapTrinhWeb.Models
{
    public class RecurringTransaction
    {
        [Key]
        public int RecurringTransactionId { get; set; }

        [Required(ErrorMessage = "Bạn phải nhập số tiền.")]
        [Range(1, int.MaxValue, ErrorMessage = "Số tiền phải lớn hơn 0.")]
        public int Amount { get; set; }

        public string? Note { get; set; }

        [Required(ErrorMessage = "Xin hãy chọn một danh mục.")]
        [Range(1, int.MaxValue, ErrorMessage = "Xin hãy chọn một mục")]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }

        public string? UserID { get; set; }

        [Required(ErrorMessage = "Xin hãy chọn một khoảng thời gian lặp lại.")]
        [Column(TypeName = "nvarchar(10)")]
        public string RecurrenceInterval { get; set; } // "weekly", "monthly", "yearly", "Daily" etcs.

        [Required(ErrorMessage = "Xin hãy nhập ngày bắt đầu.")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Xin hãy nhập ngày Kết Thúc.")]
        public DateTime EndDate { get; set; }

        public DateTime? LastProcessedDate { get; set; }

        [NotMapped]
        public string? CategoryNameWithIcon
        {
            get
            {
                return Category == null ? "" : Category.Icon + " " + Category.Name;
            }
        }

        [NotMapped]
        public string? FormatAmount
        {
            get
            {
                return ((Category == null || Category.Type == "Expense") ? " - " : " + ") + Amount.ToString("C0");
            }
        }
    }
}