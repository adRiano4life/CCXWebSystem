namespace WebStudio.Models
{
    public class FileModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string CardId { get; set; }
        public virtual Card Card { get; set; }
    }
}