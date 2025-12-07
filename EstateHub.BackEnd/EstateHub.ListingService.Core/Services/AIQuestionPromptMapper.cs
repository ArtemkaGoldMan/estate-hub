namespace EstateHub.ListingService.Core.Services;

public static class AIQuestionPromptMapper
{
    private static readonly Dictionary<string, string> QuestionPrompts = new()
    {
        {
            "schools",
            "What schools and kindergartens are nearby? Please provide exact addresses with street names and numbers, school names, and approximate distances from the location."
        },
        {
            "parks",
            "What parks and recreational areas are nearby? Please provide exact addresses with street names and numbers, park names, and approximate distances from the location."
        },
        {
            "transportation",
            "What public transportation options are available nearby? Please provide exact metro stations, bus stops, and tram stops with their addresses (street names and numbers), station/stop names, and approximate distances from the location."
        },
        {
            "hospitals",
            "What hospitals, clinics, and medical facilities are nearby? Please provide exact addresses with street names and numbers, facility names, and approximate distances from the location."
        },
        {
            "shopping",
            "What shopping centers, supermarkets, and stores are nearby? Please provide exact addresses with street names and numbers, store names, and approximate distances from the location."
        },
        {
            "restaurants",
            "What restaurants, cafes, and dining options are nearby? Please provide exact addresses with street names and numbers, restaurant/cafe names, and approximate distances from the location."
        },
        {
            "gyms",
            "What gyms, fitness centers, and sports facilities are nearby? Please provide exact addresses with street names and numbers, facility names, and approximate distances from the location."
        },
        {
            "parking",
            "What parking facilities, parking lots, and parking garages are nearby? Please provide exact addresses with street names and numbers, facility names, and approximate distances from the location."
        }
    };

    public static string GetPromptForQuestion(string questionId)
    {
        if (QuestionPrompts.TryGetValue(questionId, out var prompt))
        {
            return prompt;
        }

        // Fallback: if question ID not found, return the question as-is
        return questionId;
    }

    public static bool IsValidQuestionId(string questionId)
    {
        return QuestionPrompts.ContainsKey(questionId);
    }
}

