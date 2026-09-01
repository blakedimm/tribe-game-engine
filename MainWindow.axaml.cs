using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TribeGameUI.Models;
using TribeGameUI.Core;
using Avalonia.Media.Imaging;
using System.IO;
using System.Reflection;

namespace TribeGameUI;

public partial class MainWindow : Window
{
    private Tribe _tribe;
    private StoryManager _story;
    private List<Choice> _validChoices = new();
    
    // Токен для остановки таймера выбора
    private CancellationTokenSource? _timerCts;
    // Токен для остановки эффекта печатной машинки
    private CancellationTokenSource? _typewriterCts;

    private static readonly IBrush BrushNormal  = new SolidColorBrush(Color.Parse("#b4a080"));
    private static readonly IBrush BrushDanger  = new SolidColorBrush(Color.Parse("#9b2c1a"));

    private readonly string[] _prologueLines =
    {
        "«Жизнь человека одинока, бедна, беспросветна, звероподобна и кратка».",
        "— Томас Гоббс",
        "Великая Стужа доказала это."
    };

    public MainWindow()
    {
        InitializeComponent();
        _tribe = new Tribe(3, 1, 5); // Поправил стартовые ресурсы
        _story = new StoryManager();
        Loaded += async (_, _) => await PlayPrologueAsync();
    }

    private async Task PlayPrologueAsync()
    {
        foreach (var (line, index) in _prologueLines.Select((l, i) => (l, i)))
        {
            PrologueCounter.Text = $"{ToRoman(index + 1)} / {ToRoman(_prologueLines.Length)}";
            CinematicText.Text = line;
            CinematicText.Opacity = 1;
            await Task.Delay(3000);
            CinematicText.Opacity = 0;
            await Task.Delay(900);
        }

        StartGameButton.Opacity = 1;
        StartGameButton.IsHitTestVisible = true;
    }

    private static string ToRoman(int n) => n switch
    {
        1 => "I", 2 => "II", 3 => "III", 4 => "IV", 5 => "V", _ => n.ToString()
    };

    private void OnStartGameClicked(object? sender, RoutedEventArgs e)
    {
        ProloguePanel.IsVisible = false;
        GamePanel.IsVisible = true;
        UpdateUI();
    }

    private void UpdateUI()
    {
        // 1. Обязательно сбрасываем старый таймер перед отрисовкой нового экрана
        _timerCts?.Cancel();

        if (_tribe.IsDead() || _story.IsStoryOver())
        {
            ShowGameOver();
            return;
        }

        StoryNode node = _story.GetCurrentNode();

        ResourceFood.Text   = _tribe.Food.ToString();
        ResourceWood.Text   = _tribe.Wood.ToString();
        ResourcePeople.Text = _tribe.People.ToString();
        ResourceMorale.Text = $"{_tribe.Morale}%";

        ResourceFood.Foreground   = _tribe.Food   <= 3 ? BrushDanger : BrushNormal;
        ResourcePeople.Foreground = _tribe.People <= 2 ? BrushDanger : BrushNormal;
        ResourceMorale.Foreground = _tribe.Morale <= 30 ? BrushDanger : new SolidColorBrush(Color.Parse("LightSkyBlue"));

        // === ПОЯВЛЕНИЕ ПАНЕЛИ РЕСУРСОВ ===
        ResourcePanel.IsVisible = _tribe.Flags.Contains("show_resources");

        bool hasSpeaker = !string.IsNullOrEmpty(node.SpeakerName);
        SpeakerTag.Text       = node.SpeakerName ?? string.Empty;
        SpeakerTag.IsVisible  = hasSpeaker;

        // Сбрасываем старую печать
        _typewriterCts?.Cancel();
        _typewriterCts = new CancellationTokenSource();
        
        // Запускаем побуквенный вывод
        _ = TypeTextAsync(node.Text, StoryText, _typewriterCts.Token);

        QuestionText.Text = node.Question ?? string.Empty;

        // --- ЗАГРУЗКА КАРТИНКИ ФОНА И СПРАЙТА ---
        LoadImageSafe(node.ImagePath, BackgroundImage);
        LoadImageSafe(node.CharacterImage, CharacterImage);

        // --- АУДИО (3 ПОТОКА) ---
        if (!string.IsNullOrEmpty(node.BgmPath))
        {
            if (node.BgmPath == "clear") AudioManager.StopBGM();
            else AudioManager.PlayBGM(node.BgmPath);
        }

        if (!string.IsNullOrEmpty(node.AmbientPath))
        {
            if (node.AmbientPath == "clear") AudioManager.StopAmbient();
            else AudioManager.PlayAmbient(node.AmbientPath);
        }

        if (!string.IsNullOrEmpty(node.MusicPath))
        {
            if (node.MusicPath == "clear") AudioManager.StopMusic();
            else AudioManager.PlayMusic(node.MusicPath);
        }

        if (!string.IsNullOrEmpty(node.VoiceSfx))
        {
            AudioManager.PlaySFX(node.VoiceSfx);
        }

        // === ПРОВЕРКА НА ТРЯСКУ ЭКРАНА ===
        if (node.Shake)
        {
            _ = ShakeScreenAsync(); // Вызываем без ожидания (await), чтобы не стопить остальной код
        }

        // --- ФИЛЬТРАЦИЯ КНОПОК ---
        _validChoices = node.Choices.Where(c =>
        {
            bool hasRequired = true;
            if (!string.IsNullOrEmpty(c.RequiredFlag))
            {
                var reqs = c.RequiredFlag.Split(',');
                hasRequired = reqs.All(r => _tribe.Flags.Contains(r.Trim()));
            }

            bool shouldHide = false;
            if (!string.IsNullOrEmpty(c.HideIfFlag))
            {
                var hides = c.HideIfFlag.Split(',');
                shouldHide = hides.Any(h => _tribe.Flags.Contains(h.Trim()));
            }

            return hasRequired && !shouldHide;
        }).ToList();

        // --- ГЕНЕРАЦИЯ КНОПОК ---
        ChoicesPanel.Children.Clear();
        for (int i = 0; i < _validChoices.Count; i++)
        {
            int currentIndex = i; 
            var btn = new Button { Content = _validChoices[currentIndex].Text };

            if (currentIndex % 2 == 0) btn.Classes.Add("ChoiceButtonStyle");
            else btn.Classes.Add("ChoiceAltButtonStyle");

            btn.Click += (sender, args) => ApplyChoice(currentIndex);
            ChoicesPanel.Children.Add(btn);
        }

        // --- ЗАПУСК ТАЙМЕРА (если он указан в JSON) ---
        if (node.TimerSeconds.HasValue && !string.IsNullOrEmpty(node.TimeoutNodeId))
        {
            ChoiceTimerBar.IsVisible = true;
            _timerCts = new CancellationTokenSource();
            StartTimerAsync(node.TimerSeconds.Value, node.TimeoutNodeId, _timerCts.Token);
        }
        else
        {
            ChoiceTimerBar.IsVisible = false;
        }
    }

    // Вспомогательный метод для загрузки картинок из вшитых ресурсов
    private void LoadImageSafe(string relativePath, Image imageControl)
    {
        // Если в JSON картинка не указана — просто оставляем прошлую (ничего не делаем)
        if (string.IsNullOrEmpty(relativePath)) return;

        // Если явно прописано "clear" — прячем картинку
        if (relativePath == "clear")
        {
            imageControl.IsVisible = false;
            return;
        }

        try
        {
            string resourceName = $"TribeGameUI.{relativePath.Replace('/', '.').Replace('\\', '.')}";
            var assembly = Assembly.GetExecutingAssembly();

            using Stream? stream = assembly.GetManifestResourceStream(resourceName);
            if (stream != null)
            {
                imageControl.Source = new Bitmap(stream);
                imageControl.IsVisible = true;
            }
            else
            {
                imageControl.IsVisible = false;
            }
        }
        catch
        {
            imageControl.IsVisible = false;
        }
    }

    // Асинхронный метод таймера
    private async void StartTimerAsync(int seconds, string timeoutNodeId, CancellationToken token)
    {
        ChoiceTimerBar.Maximum = seconds * 100; // 100 тиков в секунду для плавности
        ChoiceTimerBar.Value = ChoiceTimerBar.Maximum;

        try
        {
            // Сжигаем полоску с шагом 10 мс
            while (ChoiceTimerBar.Value > 0)
            {
                await Task.Delay(10, token);
                ChoiceTimerBar.Value -= 1;
            }

            // Если время вышло и Task.Delay не выбросил ошибку отмены
            _story.ProcessChoice(new Choice { NextNodeId = timeoutNodeId });
            UpdateUI();
        }
        catch (TaskCanceledException)
        {
            // Таймер прерван игроком (он успел нажать кнопку)
        }
    }

    private async void ApplyChoice(int index)
    {
        if (_validChoices.Count <= index) return;

        // Игрок успел кликнуть — отменяем таймер!
        _timerCts?.Cancel();

        Choice choice = _validChoices[index];
        
        // Стандартное применение ресурсов от выбора
        _tribe.ApplyChanges(choice.FoodDelta, choice.WoodDelta, choice.PeopleDelta, choice.AddFlag);
        _tribe.Morale += choice.MoraleDelta; // Применяем дельту морали
        
        // === ЛОГИКА КОНЦА ДНЯ: ГОЛОД И ХОЛОД ===
        if (choice.IsEndOfDay)
        {
            // 1. Еда: каждый человек съедает 1 единицу
            _tribe.Food -= _tribe.People;
            if (_tribe.Food < 0)
            {
                int starvingPeople = System.Math.Abs(_tribe.Food);
                _tribe.Morale -= (starvingPeople * 15); // Штраф -15% за каждого голодного
                _tribe.Food = 0; 
            }

            // 2. Дрова: сжигаем 1 единицу на обогрев ночью
            _tribe.Wood -= 1;
            if (_tribe.Wood < 0)
            {
                _tribe.Morale -= 20; // Штраф -20% за обморожение
                _tribe.Wood = 0;
            }
        }

        // Ограничиваем мораль рамками от 0 до 100
        _tribe.Morale = System.Math.Clamp(_tribe.Morale, 0, 100);
        
        if (!string.IsNullOrEmpty(choice.SfxPath))
        {
            AudioManager.PlaySFX(choice.SfxPath);
        }

        if (!string.IsNullOrEmpty(choice.TrustNotification))
        {
            ShowNotificationAsync(choice.TrustNotification);
        }
        
        // --- ПРОВЕРКА НА СМЕНУ ДНЯ (ПЛАВНОЕ ЗАТЕМНЕНИЕ) ---
        if (!string.IsNullOrEmpty(choice.LoadFile))
        {
            FadeOverlay.Opacity = 1;      // Экран темнеет
            await Task.Delay(1500);       // Ждем полторы секунды
            
            _story.ProcessChoice(choice); // Грузим новый сюжет
            UpdateUI();                   // Обновляем текст и фон
            
            FadeOverlay.Opacity = 0;      // Экран плавно светлеет
        }
        else
        {
            _story.ProcessChoice(choice);
            UpdateUI();
        }
    }

    // Асинхронный метод появления плашки в углу экрана
    private async void ShowNotificationAsync(string text)
    {
        NotificationText.Text = text;
        NotificationPanel.Opacity = 1;

        try
        {
            await Task.Delay(3500); // Висит 3.5 секунды
            NotificationPanel.Opacity = 0; // И плавно исчезает
        }
        catch { }
    }

    private void ShowGameOver()
    {
        SpeakerTag.Text       = "ИСТОРИЯ ОКОНЧЕНА";
        SpeakerTag.IsVisible  = true;
        ChoiceTimerBar.IsVisible = false;

        StoryText.Text = _tribe.IsDead() || _tribe.Morale <= 0
            ? "Племя погибло во льдах. Лишь ветер помнит их имена."
            : "Вы пережили эти дни. История ждёт продолжения.";

        QuestionText.Text = string.Empty;
        ChoicesPanel.Children.Clear(); 
    }
    // Асинхронный метод эффекта печатной машинки
    private async Task TypeTextAsync(string fullText, TextBlock targetBlock, CancellationToken token)
    {
        targetBlock.Text = "";
        foreach (char c in fullText)
        {
            if (token.IsCancellationRequested)
            {
                // Если игрок нажал кнопку раньше времени - выводим весь текст сразу
                targetBlock.Text = fullText;
                return;
            }
            
            targetBlock.Text += c;
            await Task.Delay(15, token); // 15 миллисекунд между буквами (очень быстро и комфортно)
        }
    }
    // Асинхронный метод тряски экрана
    private async Task ShakeScreenAsync(int durationMs = 400, int intensity = 12)
    {
        var rnd = new System.Random();
        int elapsed = 0;

        // Берем самый главный элемент окна (Grid), чтобы не лезть в XAML
        var rootControl = (Avalonia.Controls.Control)this.Content;
        
        // Создаем трансформацию прямо в коде, если её еще нет
        if (rootControl.RenderTransform is not TranslateTransform transform)
        {
            transform = new TranslateTransform();
            rootControl.RenderTransform = transform;
        }

        while (elapsed < durationMs)
        {
            // Случайно сдвигаем экран по X и Y
            transform.X = rnd.Next(-intensity, intensity);
            transform.Y = rnd.Next(-intensity, intensity);
            
            await Task.Delay(20); // Пауза между рывками
            elapsed += 20;
        }
        
        // Возвращаем экран ровно на место
        transform.X = 0;
        transform.Y = 0;
    }
}