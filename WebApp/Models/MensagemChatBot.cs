namespace WebApp.Models;
 
public class MensagemChatBot
{
    public string message { get; set; }
    public string sender { get; set; }
    public string data { get; set; }
    public TIPO type { get; set; } //Só serve para o frontEnd

    public MensagemChatBot(string sender, string message)
    {
        this.sender = sender;
        this.message = message;
        this.data = DateTime.Now.ToString("dd-MM-yyyy");
    }
    
    public MensagemChatBot(string sender, string message,TIPO type)
    {
        this.sender = sender;
        this.message = message;
        this.data = DateTime.Now.ToString("dd-MM-yyyy");
        this.type = type;
    }
}

public enum TIPO
{
    SUCESSO,
    AVISO,
    ERRO,
    DISPLAY
}
