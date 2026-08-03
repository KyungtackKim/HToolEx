using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using ToolRunner.Models;

namespace ToolRunner.Services;

/// <summary>
///     loads and saves <see cref="AppSettings" /> as JSON under the per-user application data
///     folder. every failure falls back to defaults rather than throwing, so a missing, locked or
///     corrupt settings file can never stop the app from starting or closing.
///     사용자 설정 JSON 저장소 (실패 시 기본값 폴백)
/// </summary>
public sealed class SettingsStore {
    // serializer options — indented for hand-editing, enums as names so the file stays readable
    private static readonly JsonSerializerOptions Options = new() {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>
    ///     full path of the settings file, exposed so the app can report where it was written.
    ///     설정 파일 전체 경로
    /// </summary>
    public string FilePath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Hantas", "ToolRunner", "settings.json");

    /// <summary>
    ///     reads the stored settings, returning defaults when nothing has been saved yet or the
    ///     file cannot be read.
    ///     저장된 설정 읽기 (없으면 기본값)
    /// </summary>
    /// <returns>stored settings, or a default instance</returns>
    public AppSettings Load() {
        // guard: a missing, locked or corrupt file must not break startup
        try {
            // nothing saved yet — hand back the defaults
            if (!File.Exists(FilePath))
                // first run
                return new AppSettings();

            // read the stored JSON
            var json = File.ReadAllText(FilePath);
            // deserialize; a file holding literal "null" yields null, so fall back to defaults
            return JsonSerializer.Deserialize<AppSettings>(json, Options) ?? new AppSettings();
        } catch (Exception) {
            // unreadable file or malformed JSON — start from defaults
            return new AppSettings();
        }
    }

    /// <summary>
    ///     writes the settings, creating the folder on first save. the payload goes to a temporary
    ///     file that is then swapped into place, so an interrupted write cannot leave a half-written
    ///     settings file behind.
    ///     설정 저장 (임시 파일 후 원자적 교체)
    /// </summary>
    /// <param name="settings">settings to persist</param>
    /// <returns>true when the file was written</returns>
    public bool Save(AppSettings settings) {
        // guard: disk, permission or serialization failures must not break the shutdown path
        try {
            // resolve the containing folder
            var directory = Path.GetDirectoryName(FilePath);

            // create it when this is the first save
            if (!string.IsNullOrEmpty(directory))
                // no-op when it already exists
                Directory.CreateDirectory(directory);

            // stage the payload next to the target file
            var temp = FilePath + ".tmp";
            // serialize and write the staged copy
            File.WriteAllText(temp, JsonSerializer.Serialize(settings, Options));
            // swap the staged copy into place, replacing any previous file
            File.Move(temp, FilePath, true);

            // report success
            return true;
        } catch (Exception) {
            // settings simply are not persisted this time
            return false;
        }
    }
}
