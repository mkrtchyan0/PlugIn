namespace PlugIn.Dal.Models
{
    public class ApplicationProject
    {
        public Guid Id { get; set; }
        public string UserLogin { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
