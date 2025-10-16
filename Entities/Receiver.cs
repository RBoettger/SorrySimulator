namespace SorrySimulator.Entities
{
    public class Receiver
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
    }
}
