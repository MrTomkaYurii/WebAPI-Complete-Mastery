//----------------------------------------------------------------
// BAD CASE
//----------------------------------------------------------------
class BadCase_Book
{
    public string Text { get; set; }
    public BadCase_ConsolePrinter Printer = new BadCase_ConsolePrinter();

    public void Print(string text)
    {
        Printer.Print(text);
    }
}
class BadCase_ConsolePrinter
{
    public void Print(string text)
    {
        Console.WriteLine(text);
    }
}

//----------------------------------------------------------------
// GOOD CASE
//----------------------------------------------------------------

interface IPrinter
{
    void Print(string text);
}

class Book
{
    public string Text { get; set; }
    public IPrinter Printer { get; set; }

    public Book(IPrinter printer)
    {
        this.Printer = printer;
    }

    public void Print()
    {
        Printer.Print(Text);
    }
}

class ConsolePrinter : IPrinter
{
    public void Print(string text)
    {
        Console.WriteLine("Друк на консолі");
    }
}

class HtmlPrinter : IPrinter
{
    public void Print(string text)
    {
        Console.WriteLine("Друк в html");
    }
}

class Program
{
    static void Main(string[] args)
    {
        // Bad Case
        Console.WriteLine("=== BAD CASE ===");
        BadCase_Book badcase_book = new BadCase_Book();
        badcase_book.Print("qwerty");

        // Good Case
        Console.WriteLine("=== GOOD CASE ===");
        Book book = new Book(new ConsolePrinter());
        book.Print();
        book.Printer = new HtmlPrinter();
        book.Print();
    }
}

