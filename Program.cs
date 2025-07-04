using System.Runtime.InteropServices;
using SixLabors.ImageSharp;

class FilterConfig
{
    public double HeightRatio { get; set; } = 0.9;
    public double WidthRatio { get; set; } = 0.9;
    public double AspectRatioTolerance { get; set; } = 0.3;
}

class Program
{
    public static void Main(string[] args)
    {
        string path = Directory.GetCurrentDirectory();
        string destination = Path.Combine(path, "FilteredImages");
        if (!Directory.Exists(destination))
        {
            Directory.CreateDirectory(destination);
        }
        Console.WriteLine("Current Directory: " + path);
        (int screenWidth, int screenHeight) = GetScreenResolution();
        double screenRatio = (double)screenWidth / screenHeight;
        var screenInfo = Tuple.Create(screenWidth, screenHeight, screenRatio);
        Console.WriteLine($"Screen Resolution: {screenWidth}x{screenHeight}");
        Console.WriteLine("Now, scanning for image files...");

        var imageFiles = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
                .Where(s => !s.StartsWith(destination + Path.DirectorySeparatorChar) && 
                            (s.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                            s.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase) ||
                            s.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                            s.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
                        
                );

        Console.WriteLine(imageFiles.Count() + " image files found.");

        var config = ParseArguments(args);
        Console.WriteLine($"参数: HeightRatio={config.HeightRatio}, WidthRatio={config.WidthRatio}, AspectRatioTolerance={config.AspectRatioTolerance}");

        ProcessImages(imageFiles, screenInfo, destination, config);
    }

    private static FilterConfig ParseArguments(string[] args)
    {
        var config = new FilterConfig();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--height" && i + 1 < args.Length && double.TryParse(args[i + 1], out double h))
            {
                config.HeightRatio = h;
                i++;
            }
            else if (args[i] == "--width" && i + 1 < args.Length && double.TryParse(args[i + 1], out double w))
            {
                config.WidthRatio = w;
                i++;
            }
            else if (args[i] == "--ratio" && i + 1 < args.Length && double.TryParse(args[i + 1], out double r))
            {
                config.AspectRatioTolerance = r;
                i++;
            }
        }
        return config;
    }

    private static void ProcessImages(IEnumerable<string>? imageFiles, Tuple<int, int, double> screenInfo, string destination, FilterConfig config)
    {
        if (imageFiles == null)
            return;
        Parallel.ForEach(imageFiles, imageFile =>
        {
            if (IsImageQualified(imageFile, screenInfo, config))
            {
                string destFile = GetUniqueFilePath(destination, Path.GetFileName(imageFile));
                File.Move(imageFile, destFile);
                Console.WriteLine($"Moved: {imageFile} -> {destFile}");
            }
        });
    }

    private static string GetUniqueFilePath(string directory, string fileName)
    {
        string name = Path.GetFileNameWithoutExtension(fileName);
        string ext = Path.GetExtension(fileName);
        string fullPath = Path.Combine(directory, fileName);
        int count = 1;
        while (File.Exists(fullPath))
        {
            fullPath = Path.Combine(directory, $"{name} ({count}){ext}");
            count++;
        }
        return fullPath;
    }

    private static bool IsImageQualified(string imageFile, Tuple<int, int, double> screenInfo, FilterConfig config)
    {
        using (var image = Image.Load(imageFile))
        {
            var height = image.Height;
            var width = image.Width;
            var imageRatio = (double)width / height;
            bool availableRatio = Math.Abs(screenInfo.Item3 - imageRatio) < config.AspectRatioTolerance;
            bool heightOk = height > screenInfo.Item2 * config.HeightRatio;
            bool widthOk = width > screenInfo.Item1 * config.WidthRatio;
            return heightOk && widthOk && availableRatio;
        }
    }

    private static (int width, int height) GetScreenResolution()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // Windows
            return (GetSystemMetrics(0), GetSystemMetrics(1));
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            // macOS
            var output = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "sh",
                Arguments = "-c \"system_profiler SPDisplaysDataType | awk '/Resolution/{print $2, $4}' | head -n1\"",
                RedirectStandardOutput = true,
                UseShellExecute = false
            });
            string result = output.StandardOutput.ReadToEnd();
            output.WaitForExit();
            var parts = result.Trim().Split(' ');
            if (parts.Length == 2 && int.TryParse(parts[0], out int w) && int.TryParse(parts[1], out int h))
                return (w, h);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            // Linux
            var output = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "sh",
                Arguments = "-c \"xrandr | grep '*' | awk '{print $1}' | head -n1\"",
                RedirectStandardOutput = true,
                UseShellExecute = false
            });
            string result = output.StandardOutput.ReadToEnd();
            output.WaitForExit();
            var parts = result.Trim().Split('x');
            if (parts.Length == 2 && int.TryParse(parts[0], out int w) && int.TryParse(parts[1], out int h))
                return (w, h);
        }
        throw new PlatformNotSupportedException("Unable to determine screen resolution on this platform.");
    }

    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int nIndex);
}