namespace DatingApp.HelperClasses
{
    public class LikesParams:PaginationParams
    {
        public int UserId { get; set; }
        public string Perdicate { get; set; }
    }
}
