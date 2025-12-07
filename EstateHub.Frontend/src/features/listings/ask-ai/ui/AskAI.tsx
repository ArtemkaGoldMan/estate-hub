import { useState, useEffect } from 'react';
import { Button, LoadingSpinner } from '../../../../shared/ui';
import { useAskAboutLocation, useRemainingAIQuestions } from '../../../../entities/listing';
import { useToast } from '../../../../shared/context/ToastContext';
import { useAuth } from '../../../../shared/context/AuthContext';
import { FaTree, FaSubway, FaTimes, FaGraduationCap, FaHospital, FaShoppingCart, FaUtensils, FaDumbbell, FaCar } from 'react-icons/fa';
import './AskAI.css';

const PREDEFINED_QUESTIONS = [
  {
    id: 'schools',
    question: 'What schools are nearby?',
    Icon: FaGraduationCap,
  },
  {
    id: 'parks',
    question: 'What parks are nearby?',
    Icon: FaTree,
  },
  {
    id: 'transportation',
    question: 'What public transportation is available?',
    Icon: FaSubway,
  },
  {
    id: 'hospitals',
    question: 'What medical facilities are nearby?',
    Icon: FaHospital,
  },
  {
    id: 'shopping',
    question: 'What shopping options are nearby?',
    Icon: FaShoppingCart,
  },
  {
    id: 'restaurants',
    question: 'What restaurants are nearby?',
    Icon: FaUtensils,
  },
  {
    id: 'gyms',
    question: 'What fitness facilities are nearby?',
    Icon: FaDumbbell,
  },
  {
    id: 'parking',
    question: 'What parking is available?',
    Icon: FaCar,
  },
];

const DAILY_LIMIT = 5;

type AskAIProps = {
  listingId: string;
};

export const AskAI = ({ listingId }: AskAIProps) => {
  const { askAboutLocation, loading } = useAskAboutLocation();
  const { remainingQuestions, refetch: refetchRemaining } = useRemainingAIQuestions();
  const { showError } = useToast();
  const { isAuthenticated } = useAuth();
  const [selectedQuestion, setSelectedQuestion] = useState<string | null>(null);
  const [answer, setAnswer] = useState<string | null>(null);

  useEffect(() => {
    if (isAuthenticated) {
      // Silently try to fetch remaining questions, but don't break if it fails
      refetchRemaining().catch(() => {
        // Silently handle errors - we'll just show default value
      });
    }
  }, [isAuthenticated, refetchRemaining]);

  const handleAskQuestion = async (questionId: string, questionText: string) => {
    if (remainingQuestions <= 0) {
      showError(`You have reached your daily limit of ${DAILY_LIMIT} questions. Please try again tomorrow.`);
      return;
    }

    try {
      setSelectedQuestion(questionText);
      setAnswer(null);
      const response = await askAboutLocation(listingId, questionId);
      setAnswer(response.answer);
      // Refresh remaining count after asking (silently handle errors)
      try {
        await refetchRemaining();
      } catch {
        // Ignore refetch errors
      }
    } catch (error) {
      // Error message is already user-friendly from the hook
      const errorMessage = error instanceof Error 
        ? error.message 
        : 'Unable to get AI response. Please try again later.';
      showError(errorMessage);
      setSelectedQuestion(null);
    }
  };

  const handleReset = () => {
    setSelectedQuestion(null);
    setAnswer(null);
  };

  const isLimitReached = remainingQuestions <= 0;
  const canAsk = !isLimitReached && !loading;

  return (
    <div className="ask-ai">
      <div className="ask-ai__header">
        <h2>Ask AI</h2>
        <p className="ask-ai__subtitle">Get information about what's nearby</p>
        {isAuthenticated && (
          <p className="ask-ai__remaining">
            {isLimitReached ? (
              <span className="ask-ai__remaining--limit">Daily limit reached (0/{DAILY_LIMIT})</span>
            ) : (
              <span className="ask-ai__remaining--available">
                {remainingQuestions} of {DAILY_LIMIT} questions remaining today
              </span>
            )}
          </p>
        )}
      </div>

      <div className="ask-ai__questions">
        {PREDEFINED_QUESTIONS.map((item) => {
          const IconComponent = item.Icon;
          return (
            <Button
              key={item.id}
              variant="outline"
              onClick={() => handleAskQuestion(item.id, item.question)}
              disabled={!canAsk}
              className="ask-ai__question-button"
            >
              <span className="ask-ai__question-icon">
                <IconComponent />
              </span>
              <span className="ask-ai__question-text">{item.question}</span>
            </Button>
          );
        })}
      </div>

      {(selectedQuestion || answer) && (
        <div className="ask-ai__response">
          <div className="ask-ai__response-header">
            <h3>Question</h3>
            <Button variant="ghost" size="sm" onClick={handleReset}>
              <FaTimes />
            </Button>
          </div>
          <div className="ask-ai__question-display">
            {selectedQuestion}
          </div>

          {loading && (
            <div className="ask-ai__loading">
              <LoadingSpinner text="AI is thinking..." />
            </div>
          )}

          {answer && !loading && (
            <div className="ask-ai__answer">
              <h3>Answer</h3>
              <div className="ask-ai__answer-content">{answer}</div>
            </div>
          )}
        </div>
      )}
    </div>
  );
};

