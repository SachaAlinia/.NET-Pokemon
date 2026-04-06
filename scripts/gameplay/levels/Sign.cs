using Game.Core;
using Game.UI;
using Godot;
using Godot.Collections;
using System;
using Logger = Game.Core.Logger;

namespace Game.Gameplay;

/// <summary>
/// Panneau qui affiche un message au joueur.
/// </summary>
[Tool]
public partial class Sign : StaticBody2D
{
    // Liste des messages affichés par le panneau.
    [Export]
    public Array<string> Messages;

    // Sign style.
    private SignType _signStyle = SignType.METAL;

    [Export]
    public SignType SignStyle
    {
        get => _signStyle;
        set
        {
            if (_signStyle != value)
            {
                _signStyle = value;
                UpdateSprite();
            }
        }
    }

    // Sprite2D pour afficher le panneau.
    private Sprite2D _sprite2D;

    // Dictionnaire des textures pour chaque style de panneau.
    private readonly Dictionary<SignType, AtlasTexture> _textures = new()
    {
        { SignType.METAL, GD.Load<AtlasTexture>("res://resources/textures/sign_metal.tres") },
        { SignType.WOOD, GD.Load<AtlasTexture>("res://resources/textures/sign_wood.tres") }
    };

    /// <summary>
    /// Initialise le panneau au démarrage.
    /// Récupère le Sprite2D et met à jour la texture.
    /// </summary>
    public override void _Ready()
    {
        _sprite2D ??= GetNode<Sprite2D>("Sprite2D");
        UpdateSprite();
    }

    /// <summary>
    /// Met à jour la texture du Sprite2D en fonction du style sélectionné.
    /// </summary>
    private void UpdateSprite()
    {
        // Récupérer le Sprite2D s'il n'est pas encore assigné.
        if (_sprite2D == null)
        {
            _sprite2D = GetNodeOrNull<Sprite2D>("Sprite2D");

            if (_sprite2D == null)
            {
                Logger.Error("Sprite2D node not found");
                return;
            }
        }

        // Appliquer la texture correspondante au style.
        if (_textures.TryGetValue(SignStyle, out var texture))
        {
            _sprite2D.Texture = texture;
        }
        else
        {
            Logger.Error($"No texture found for {SignStyle}");
            _sprite2D.Texture = null;
        }
    }

    /// <summary>
    /// Affiche les messages du panneau via le gestionnaire de messages.
    /// </summary>
    public void PlayMessage()
    {
        MessageManager.PlayText([.. Messages]);
    }
}
