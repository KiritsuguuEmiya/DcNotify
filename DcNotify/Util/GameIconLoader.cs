using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using Flurl.Http;

namespace Dnc.Util;

public static class GameIconLoader
{
    private static Guid? pngEncoderGuid;

    public static async Task<Bitmap?> LoadAsync(uint iconId)
    {
        if (iconId == 0)
            return null;

        var fromGame = await TryLoadFromGameAsync(iconId);
        if (fromGame != null)
            return fromGame;

        return await TryLoadFromXivApiAsync(iconId);
    }

    private static Task<Bitmap?> TryLoadFromGameAsync(uint iconId)
    {
        var tcs = new TaskCompletionSource<Bitmap?>(TaskCreationOptions.RunContinuationsAsynchronously);
        Service.Framework.RunOnFrameworkThread(() =>
        {
            _ = LoadFromGameOnFramework(iconId, tcs);
        });
        return tcs.Task;
    }

    private static async Task LoadFromGameOnFramework(uint iconId, TaskCompletionSource<Bitmap?> tcs)
    {
        IDalamudTextureWrap? wrap = null;
        try
        {
            var texture = Service.TextureProvider.GetFromGameIcon(new GameIconLookup(iconId));
            await texture.RentAsync();

            if (!texture.TryGetWrap(out wrap, out var error) || wrap == null)
            {
                if (error != null)
                    Service.PluginLog.Debug(error, $"Game icon {iconId} unavailable.");
                tcs.TrySetResult(null);
                return;
            }

            using var ms = new MemoryStream();
            await Service.TextureReadback.SaveToStreamAsync(
                wrap,
                GetPngEncoderGuid(),
                ms,
                leaveWrapOpen: true,
                leaveStreamOpen: true);

            ms.Position = 0;
            tcs.TrySetResult(new Bitmap(ms));
        }
        catch (Exception ex)
        {
            Service.PluginLog.Debug(ex, $"Failed to load game icon {iconId}.");
            tcs.TrySetResult(null);
        }
        finally
        {
            wrap?.Dispose();
        }
    }

    private static async Task<Bitmap?> TryLoadFromXivApiAsync(uint iconId)
    {
        try
        {
            var bytes = await BuildXivApiIconUrl(iconId).GetBytesAsync();
            using var stream = new MemoryStream(bytes);
            return new Bitmap(stream);
        }
        catch (Exception ex)
        {
            Service.PluginLog.Debug(ex, $"Failed to load icon {iconId} from XIVAPI.");
            return null;
        }
    }

    private static Guid GetPngEncoderGuid()
    {
        if (pngEncoderGuid.HasValue)
            return pngEncoderGuid.Value;

        var encoder = Service.TextureReadback.GetSupportedImageEncoderInfos()
            .FirstOrDefault(e => e.Extensions.Any(ext =>
                ext.Equals("png", StringComparison.OrdinalIgnoreCase)
                || ext.Equals(".png", StringComparison.OrdinalIgnoreCase)));

        if (encoder == null)
            throw new InvalidOperationException("PNG encoder not found.");

        pngEncoderGuid = encoder.ContainerGuid;
        return pngEncoderGuid.Value;
    }

    private static string BuildXivApiIconUrl(uint iconId)
    {
        var iconText = iconId.ToString().PadLeft(6, '0');
        var directory = iconText[..3] + "000";
        var path = $"ui/icon/{directory}/{iconText}.tex";
        return $"https://v2.xivapi.com/api/asset?path={Uri.EscapeDataString(path)}&format=png";
    }
}
