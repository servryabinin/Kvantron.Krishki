using System;
using System.IO;
using OpenCvSharp;

public static class CycleImageSaver
{
    public static string BaseFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Дефектные крышки");
    public static string CurrentCycleFolder = "";
    public static int CycleHours = 6; // Значение подставляется из cycleUpDown

    public static void Init()
    {
        Directory.CreateDirectory(BaseFolder);
        EnsureCycleFolder();
    }

    public static void SetCycleHours(int hours)
    {
        CycleHours = Math.Max(1, Math.Min(24, hours));
        EnsureCycleFolder();
    }

    public static void EnsureCycleFolder()
    {
        Directory.CreateDirectory(BaseFolder);

        var folders = Directory.GetDirectories(BaseFolder);
        DateTime lastFolderTime = DateTime.MinValue;

        foreach (var f in folders)
        {
            string name = Path.GetFileName(f);

            if (DateTime.TryParseExact(name, "dd-MM-yyyy HH-mm",
                null, System.Globalization.DateTimeStyles.None,
                out DateTime parsed))
            {
                if (parsed > lastFolderTime)
                    lastFolderTime = parsed;
            }
        }

        DateTime now = DateTime.Now;

        if (lastFolderTime != DateTime.MinValue &&
            lastFolderTime.Date == now.Date)
        {
            double hoursPassed = (now - lastFolderTime).TotalHours;

            if (hoursPassed >= CycleHours)
            {
                CreateNewCycleFolder(now);
            }
            else
            {
                CurrentCycleFolder = Path.Combine(
                    BaseFolder,
                    lastFolderTime.ToString("dd-MM-yyyy HH-mm")
                );
            }
        }
        else
        {
            CreateNewCycleFolder(now);
        }

        Directory.CreateDirectory(Path.Combine(CurrentCycleFolder, "OK"));
        Directory.CreateDirectory(Path.Combine(CurrentCycleFolder, "NG"));
    }

    private static void CreateNewCycleFolder(DateTime time)
    {
        string name = time.ToString("dd-MM-yyyy HH-mm");
        CurrentCycleFolder = Path.Combine(BaseFolder, name);

        Directory.CreateDirectory(CurrentCycleFolder);
        Directory.CreateDirectory(Path.Combine(CurrentCycleFolder, "OK"));
        Directory.CreateDirectory(Path.Combine(CurrentCycleFolder, "NG"));
    }

    public static void Save(Mat image, bool isNG, bool allowOk, bool allowNg)
    {
        try
        {
            if (image == null || image.Empty()) return;

            EnsureCycleFolder();

            if (!isNG && !allowOk) return;
            if (isNG && !allowNg) return;

            string subfolder = isNG ? "NG" : "OK";

            string fileName = $"{(isNG ? "NG" : "OK")}_{DateTime.Now:yyyyMMdd_HHmmss_fff}.png";
            string path = Path.Combine(CurrentCycleFolder, subfolder, fileName);

            image.SaveImage(path);
        }
        catch { }
    }
}
