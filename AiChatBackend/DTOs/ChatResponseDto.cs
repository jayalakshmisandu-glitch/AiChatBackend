namespace AiChatBackend.DTOs
{
   
    //public class ChatRequestDto
    //{
        
    //    public string Message { get; set; } = null!;
    //}

    public class CreateChatRequestDto
    {
        public string? Title { get; set; }
    }

    public class UpdateChatTitleRequestDto
    {
        public string Title { get; set; } = null!;
    }

    public class ChatResponseDto
    {
        public string Response { get; set; } = null!;
    }
}