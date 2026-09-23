namespace SomnusNet.Data
{
    public static class BlobKinds
    {
        public static BlobKind Normalize(BlobKind kind) => (int)kind switch
        {
            5 => BlobKind.FourOhFour,
            6 => BlobKind.Blaze,
            _ => kind
        };
    }
}
