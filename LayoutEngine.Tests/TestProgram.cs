using System.Threading.Tasks;

class TestProgram
{
    public static async Task Main(string[] args)
    {
        await new PublicationTests().Test_That_The_Extracted_Driver_Is_Resolved();
    }
}
