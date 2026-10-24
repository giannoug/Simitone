using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FSO.Client;
using FSO.Client.UI.Controls;
using FSO.Client.UI.Framework;
using FSO.Common;
using FSO.Common.Rendering.Framework.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Simitone.Client.UI.Controls;
using Simitone.Client.UI.Model;

namespace Simitone.Client.UI.Panels.Options
{
    public sealed class UIOptionsAlert : UIMobileDialog
    {
        private GlobalSettings Settings;
        private OptionsDraft Draft;
        private HashSet<Point> WindowedSizes;
        private HashSet<Point> FullscreenSizes;
        private readonly UITextBox[] Values = new UITextBox[6];
        private readonly UILabel[] Labels = new UILabel[5];
        private readonly UILabel Note;
        private readonly UIBigButton ApplyButton;
        private readonly UIBigButton CancelButton;
        private readonly UIBigButton ModeButton;
        public event Action Closed;

        private sealed record OptionsDraft(byte FX, byte Music, byte Voice, byte Ambience, int Width, int Height, bool Windowed);

        public UIOptionsAlert(GlobalSettings settings)
        {
            Settings = settings;
            Draft = ReadSettings();
            Caption = "Options";
            var scale = FSOEnvironment.DPIScaleFactor;
            FullscreenSizes = GameFacade.GraphicsDevice.Adapter.SupportedDisplayModes
                .Where(m => m.Width >= 800 * scale && m.Height >= 600 * scale)
                .Select(m => new Point((int)(m.Width / scale), (int)(m.Height / scale))).ToHashSet();
            WindowedSizes = new HashSet<Point>(FullscreenSizes) { new(800, 600), new(settings.GraphicsWidth, settings.GraphicsHeight), new(Draft.Width, Draft.Height) };
            var names = new[] { "Effects (0–10)", "Music (0–10)", "Voices (0–10)", "Ambience (0–10)", "Width / height" };
            for (int i = 0; i < Labels.Length; i++)
            {
                Labels[i] = new UILabel { Caption = names[i] };
                Labels[i].CaptionStyle = Labels[i].CaptionStyle.Clone();
                Labels[i].CaptionStyle.Size = 17;
                Labels[i].CaptionStyle.Color = UIStyle.Current.Text;
                Add(Labels[i]);
            }
            var initial = new[] { (int)Draft.FX, Draft.Music, Draft.Voice, Draft.Ambience, Draft.Width, Draft.Height };
            for (int i = 0; i < Values.Length; i++)
            {
                Values[i] = new UITextBox { CurrentText = initial[i].ToString(CultureInfo.InvariantCulture), MaxChars = i < 4 ? 2 : 5 };
                Values[i].SetSize(80, 28);
                Add(Values[i]);
            }
            ModeButton = Button(Draft.Windowed ? "Windowed" : "Fullscreen", () =>
            {
                Draft = Draft with { Windowed = !Draft.Windowed };
                ModeButton.Caption = Draft.Windowed ? "Windowed" : "Fullscreen";
            });
            ApplyButton = Button("Apply", Apply);
            CancelButton = Button(GameFacade.Strings.GetString("142", "1") ?? "Cancel", Close);
            Note = new UILabel { Caption = "Display changes need a restart. Use a supported display size or your current window size.", Wrapped = true };
            Note.CaptionStyle = Note.CaptionStyle.Clone();
            Note.CaptionStyle.Size = 12;
            Note.CaptionStyle.Color = UIStyle.Current.Text;
            Add(Note);
            Layout();
        }

        private UIBigButton Button(string caption, Action action)
        {
            var button = new UIBigButton(false) { Caption = caption, ScaleX = 0.45f, ScaleY = 0.45f };
            button.OnButtonClick += b => action();
            Add(button);
            return button;
        }

        private OptionsDraft ReadSettings() => new(Settings.FXVolume, Settings.MusicVolume, Settings.VoxVolume,
            Settings.AmbienceVolume, Settings.PendingDisplayOptions?.Width ?? Settings.GraphicsWidth,
            Settings.PendingDisplayOptions?.Height ?? Settings.GraphicsHeight, Settings.PendingDisplayOptions?.Windowed ?? Settings.Windowed);

        private void WriteSettings(OptionsDraft value)
        {
            Settings.FXVolume = value.FX;
            Settings.MusicVolume = value.Music;
            Settings.VoxVolume = value.Voice;
            Settings.AmbienceVolume = value.Ambience;
            Settings.PendingDisplayOptions = value.Width == Settings.GraphicsWidth && value.Height == Settings.GraphicsHeight && value.Windowed == Settings.Windowed
                ? null : (value.Width, value.Height, value.Windowed);
        }

        private bool TryApply(out string error)
        {
            if (Draft.FX > 10 || Draft.Music > 10 || Draft.Voice > 10 || Draft.Ambience > 10)
            {
                error = "Volumes must be between 0 and 10.";
                return false;
            }
            var sizes = Draft.Windowed ? WindowedSizes : FullscreenSizes;
            if (Draft.Width < 800 || Draft.Height < 600 || !sizes.Contains(new Point(Draft.Width, Draft.Height)))
            {
                error = "Choose a supported display size (at least 800 × 600).";
                return false;
            }
            var previous = ReadSettings();
            var pendingDisplay = Settings.PendingDisplayOptions;
            WriteSettings(Draft);
            if (!Settings.TrySave(out error))
            {
                WriteSettings(previous);
                Settings.PendingDisplayOptions = pendingDisplay;
                return false;
            }
            if (GameFacade.Game is SimitoneGame game)
            {
                game.ApplyAudioOptions();
            }
            return true;
        }

        private void Apply()
        {
            var volumes = new byte[4];
            string error = "Enter whole-number volumes (0–10) and a valid width and height.";
            for (int i = 0; i < volumes.Length; i++)
            {
                if (!byte.TryParse(Values[i].CurrentText, NumberStyles.Integer, CultureInfo.InvariantCulture, out volumes[i]))
                {
                    ShowError(error);
                    return;
                }
            }
            if (!int.TryParse(Values[4].CurrentText, out var width) || !int.TryParse(Values[5].CurrentText, out var height))
            {
                ShowError(error);
                return;
            }
            Draft = new OptionsDraft(volumes[0], volumes[1], volumes[2], volumes[3], width, height, Draft.Windowed);
            if (TryApply(out error)) Close();
            else ShowError(error);
        }

        private void ShowError(string message)
        {
            UIMobileAlert alert = null;
            alert = new UIMobileAlert(new UIAlertOptions
            {
                Title = "Options were not applied",
                Message = message,
                Buttons = UIAlertButton.Ok(b => alert.Close())
            });
            UIScreen.GlobalShowDialog(alert, true);
        }

        private void Layout()
        {
            SetHeight(Math.Min(480, ScrHeight - 16));
            var step = Math.Min(38f, (Height - 220f) / 5f);
            var inputX = Width * 0.55f;
            for (int i = 0; i < Labels.Length; i++) Labels[i].Position = new Vector2(30, 90 + i * step);
            for (int i = 0; i < 4; i++) Values[i].Position = new Vector2(inputX, 90 + i * step);
            Values[4].Position = new Vector2(inputX, 90 + 4 * step);
            Values[5].Position = new Vector2(inputX + 90, 90 + 4 * step);
            ModeButton.Position = new Vector2(30, 90 + 5 * step);
            Note.Position = new Vector2(30, Height - 96);
            Note.Size = new Vector2(Width - 60, 40);
            ApplyButton.Position = new Vector2(Width / 2 - 160, Height - 50);
            CancelButton.Position = new Vector2(Width / 2 + 20, Height - 50);
        }

        public override void GameResized()
        {
            base.GameResized();
            Layout();
        }

        public override void Update(UpdateState state)
        {
            base.Update(state);
            if (state.NewKeys.Contains(Keys.Escape)) Close();
        }

        public override void Removed()
        {
            base.Removed();
            Closed?.Invoke();
        }
    }
}
