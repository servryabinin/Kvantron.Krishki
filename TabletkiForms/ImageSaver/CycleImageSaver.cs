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

    public static void Save(Mat image, bool isNG, bool allowOk, bool allowNg, float generalCount)
    {
        try
        {
            if (image == null || image.Empty()) return;

            if (!isNG && !allowOk) return;
            if (isNG && !allowNg) return;

            EnsureCycleFolder();

            string subfolder = isNG ? "NG" : "OK";
            string subfolderPath = Path.Combine(CurrentCycleFolder, subfolder);

            // Создаём подкаталог, если его нет
            if (!Directory.Exists(subfolderPath))
                Directory.CreateDirectory(subfolderPath);

            // Получаем список файлов, сортируем по дате создания
            var files = new DirectoryInfo(subfolderPath)
                            .GetFiles("*.png")
                            .OrderBy(f => f.CreationTime)
                            .ToList();

            // Если больше 30000 файлов, удаляем самые старые
            while (files.Count >= 30000)
            {
                try
                {
                    files[0].Delete();
                    files.RemoveAt(0);
                }
                catch { break; } // На всякий случай, если файл нельзя удалить
            }

            // Генерируем имя нового файла
            string fileName = $"{(isNG ? "NG" : "OK")}_{DateTime.Now:yyyyMMdd_HHmmss_fff}_{generalCount}.jpg";
            string path = Path.Combine(subfolderPath, fileName);

            // Сохраняем изображение в JPEG с качеством 90%
            Cv2.ImWrite(path, image, new ImageEncodingParam(ImwriteFlags.JpegQuality, 90));

        }
        catch { /* Игнорируем ошибки сохранения */ }
    }

}
