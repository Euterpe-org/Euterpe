namespace Euterpe.Common;

public static class DependencyConstants
{
    public static class MelonLoader
    {
        public const string Version = "0.7.2";
        public const string Url = $"{EuterpeUrls.Dependencies.BaseUrl}MelonLoader/{Version}/MelonLoader.x64.zip";
        public const string ZipHash = "B1B33C807DB430E870FD5A6282CFF3FFAE6869A9D714939E36F75AE4C5D72B27CA172E5492FA9F804D26233323462582A15715BDB52439784D70F9232A0AE124";
    }

    public static class UnityRuntime
    {
        public const string Version = "2019.4.41";
        public const string Url = $"{EuterpeUrls.Dependencies.BaseUrl}UnityDependencies/{Version}/Managed.zip";
        public const string ZipHash = "1F16EB28548976335E38F53BCAF2365B8FE466EDAAF4858BD06A9726F3EF0D9D2BCA1C10D38A15ED6B5AE02466088FD14EE39859B36114220211BC99ECAF18A4";
    }

    public static class Cpp2IL
    {
        public const string Version = "2022.1.0-pre-release.21";
        public const string ExecutableUrl = $"{EuterpeUrls.Dependencies.BaseUrl}Cpp2IL/{Version}/Cpp2IL-{Version}-Windows.exe";
        public const string PluginUrl = $"{EuterpeUrls.Dependencies.BaseUrl}Cpp2IL/{Version}/Cpp2IL.Plugin.StrippedCodeRegSupport.dll";
        public const string ExecutableHash = "58937CB414501E427656D16C68368269A8ECF04EB7FA8B65831E24148579A6914832769EB943158552D04D54CDD54DB103A09714FA9C2EE106C9C4084887BCA7";
        public const string PluginHash = "0E16B5C875A408182D77CA6F3714953B9A03652D0648F6A17E42E8727C2A10F73EFA8147A6B9195F1DEF2C24E32A60DCAD873229828A886390B1EC6B0E040D5A";
    }
}