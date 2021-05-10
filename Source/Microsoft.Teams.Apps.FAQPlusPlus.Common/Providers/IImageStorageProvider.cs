namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers
{
    using System.IO;
    using System.Threading.Tasks;

    public interface IImageStorageProvider
    {
        Task<string> UploadAsync(Stream stream, string fileName);
    }
}
