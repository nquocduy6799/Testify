namespace Testify.Shared.DTOs.Marketplace
{
    public class TemplateDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public string AuthorAvatar { get; set; } = string.Empty; // URL ảnh
        public string CategoryName { get; set; } = string.Empty;
        
        // Thống kê (Mapping từ file types.ts)
        public int Stars { get; set; }
        public int Clones { get; set; }
        public int Views { get; set; }

        // Logic kinh doanh (Free/Premium)
        public string PriceType { get; set; } = "Free"; // "Free" hoặc "Premium"
        public decimal PriceAmount { get; set; } = 0;
        public bool IsOwned { get; set; } = false; // User đã mua/tải chưa?

        public List<string> Tags { get; set; } = new();
        public DateTime UpdatedAt { get; set; }
    }

    // DTO để gửi yêu cầu Clone về Server
    public class CloneTemplateRequest
    {
        public int TemplateId { get; set; }
        public int TargetProjectId { get; set; }
    }
}
