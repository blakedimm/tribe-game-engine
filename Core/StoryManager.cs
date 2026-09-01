using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using TribeGameUI.Models;

namespace TribeGameUI.Core;

public class StoryManager
{
    private List<StoryNode> _nodes = new();
    private StoryNode _currentNode;

    public StoryManager()
    {
        LoadStoryFile("story.json");
        
        // Теперь берем просто первый узел из файла, как бы он ни назывался
        _currentNode = _nodes.FirstOrDefault(); 
    }

    // Метод для загрузки нового файла JSON из встроенных ресурсов (EXE)
    private void LoadStoryFile(string fileName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        string resourceName = $"TribeGameUI.{fileName}"; 

        using Stream? stream = assembly.GetManifestResourceStream(resourceName);
        if (stream != null)
        {
            using StreamReader reader = new StreamReader(stream);
            string jsonString = reader.ReadToEnd();
            _nodes = JsonSerializer.Deserialize<List<StoryNode>>(jsonString) ?? new();
        }
    }

    public StoryNode GetCurrentNode()
    {
        return _currentNode;
    }

    // Обновленный метод перехода
    public void ProcessChoice(Choice choice)
    {
        // Если в кнопке прописан новый файл - грузим его!
        if (!string.IsNullOrEmpty(choice.LoadFile))
        {
            LoadStoryFile(choice.LoadFile);
        }

        // Прыгаем на нужный узел (или на самый первый в новом файле, если ID не указан)
        if (!string.IsNullOrEmpty(choice.NextNodeId))
        {
            _currentNode = _nodes.FirstOrDefault(n => n.Id == choice.NextNodeId);
        }
        else
        {
            _currentNode = _nodes.FirstOrDefault();
        }
    }

    public bool IsStoryOver()
    {
        return _currentNode == null || _currentNode.Choices.Count == 0;
    }
}