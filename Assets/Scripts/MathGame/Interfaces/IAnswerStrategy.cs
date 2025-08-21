using System;
using MathGame.Models;
using UnityEngine;

namespace MathGame.Interfaces
{
    public interface IAnswerStrategy
    {
        void Initialize(Question question, Action<int> onAnswerCallback);
        GameObject CreateAnswerUI(Transform parent);
        void Cleanup();
    }
}