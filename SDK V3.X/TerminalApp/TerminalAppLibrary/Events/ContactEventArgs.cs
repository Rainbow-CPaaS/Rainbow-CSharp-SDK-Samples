using Rainbow.Model;

public class ContactEventArgs: EventArgs
{
    public ContactEventArgs(Contact contact) { Contact = contact; }

    public Contact Contact { get; set; }
}