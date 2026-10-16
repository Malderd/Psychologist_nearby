using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Psychologist_nearby.Data;

namespace Psychologist_nearby.Tests;

public class TestModelTests
{
    [Fact] // Тест 18: контракт модели TestInterpretation
    public void TestInterpretation_AllFields_AreAssigned()
    {
        var interp = new TestInterpretation
        {
            MinScore = 0,
            MaxScore = 4,
            StatusText = "Минимум",
            RecommendationText = "Наблюдение"
        };

        Assert.Equal(0, interp.MinScore);
        Assert.Equal(4, interp.MaxScore);
        Assert.Equal("Минимум", interp.StatusText);
        Assert.Equal("Наблюдение", interp.RecommendationText);
    }
}
