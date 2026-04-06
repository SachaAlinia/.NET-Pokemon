using System.IO;
using System.Threading.Tasks;
using Godot;
using HttpClient = System.Net.Http.HttpClient; // Outil pour aller sur Internet.

namespace Game.Core;

public static class Modules
{
    /// <summary>
    /// Vérifie si le joueur vient d'appuyer sur une des flèches directionnelles.
    /// </summary>
    /// <returns>Vrai si une touche vient d'être pressée.</returns>
    public static bool IsActionJustPressed()
    {
        // '||' signifie "OU". On regarde si Haut OU Bas OU Gauche OU Droite est pressé.
        return Input.IsActionJustPressed("ui_up") || Input.IsActionJustPressed("ui_down") || Input.IsActionJustPressed("ui_left") || Input.IsActionJustPressed("ui_right");
    }

    /// <summary>
    /// Vérifie si le joueur maintient une touche directionnelle enfoncée.
    /// </summary>
    public static bool IsActionPressed()
    {
        return Input.IsActionPressed("ui_up") || Input.IsActionPressed("ui_down") || Input.IsActionPressed("ui_left") || Input.IsActionPressed("ui_right");
    }

    /// <summary>
    /// Vérifie si le joueur vient de relâcher une touche directionnelle.
    /// </summary>
    public static bool IsActionJustReleased()
    {
        return Input.IsActionJustReleased("ui_up") || Input.IsActionJustReleased("ui_down") || Input.IsActionJustReleased("ui_left") || Input.IsActionJustReleased("ui_right");
    }

    /// <summary>
    /// Convertit une position en pixels (Vector2) en position sur la grille (Vector2I).
    /// Exemple : si on est au pixel 32, et que GRID_SIZE est 16, on est sur la case 2.
    /// </summary>
    public static Vector2I ConvertVector2ToVector2I(Vector2 vector)
    {
        return new Vector2I((int)vector.X / Globals.GRID_SIZE, (int)vector.Y / Globals.GRID_SIZE);
    }

    /// <summary>
    /// Fait l'inverse : transforme une case de grille en position en pixels.
    /// </summary>
    public static Vector2 ConvertVector2IToVector2(Vector2I vector)
    {
        return new Vector2I(vector.X * Globals.GRID_SIZE, vector.Y * Globals.GRID_SIZE);
    }

    // Un client HTTP pour discuter avec des sites web (comme PokeApi).
    private static readonly HttpClient httpClient = new HttpClient();

    /// <summary>
    /// Tâche Asynchrone qui va chercher des informations sur les Pokémon sur Internet.
    /// </summary>
    /// <param name="url">L'adresse du site web (PokeApi).</param>
    public static async Task<Variant> FetchDataFromPokeApi(string url)
    {
        try
        {
            // On envoie la requête et on ATTEND (await) la réponse.
            var response = await httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode) // Si le site répond par une erreur (ex: 404).
            {
                Logger.Error($"Api Error: failed fetching {url} -> {response.StatusCode}");
                return default;
            }

            // On lit le texte reçu (format JSON) et on demande à Godot de le transformer en données utilisables.
            var json = await response.Content.ReadAsStringAsync();
            return Json.ParseString(json);
        }
        catch (System.Exception ex) // En cas de gros plantage (pas d'internet, etc.).
        {
            Logger.Error($"Api Error: failed fetching {url} -> {ex.Message}");
            return default;
        }
    }

    /// <summary>
    /// Télécharge l'image d'un Pokémon depuis Internet et l'enregistre sur ton ordinateur.
    /// </summary>
    public static async Task<string> DownloadSprite(string imageUrl, string saveFolderPath, string fileName)
    {
        if (string.IsNullOrEmpty(imageUrl)) return null;

        // 'ProjectSettings.GlobalizePath' transforme un chemin Godot (res://) en vrai chemin Windows (C:\...).
        string fullSavePath = ProjectSettings.GlobalizePath($"{saveFolderPath}{fileName}");
        string resourcePath = $"{saveFolderPath}{fileName}";

        try
        {
            // On télécharge les données de l'image (en octets / bytes).
            byte[] imageBytes = await httpClient.GetByteArrayAsync(imageUrl);
            // On écrit ces données dans un fichier sur le disque dur.
            File.WriteAllBytes(fullSavePath, imageBytes);
            return resourcePath;
        }
        catch (System.Exception e)
        {
            Logger.Error($"Failed to download sprite from {imageUrl} to {resourcePath}: {e.Message}");
            return null;
        }
    }
}