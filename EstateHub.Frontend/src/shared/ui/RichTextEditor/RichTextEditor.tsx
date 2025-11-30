import { useRef, useEffect, useState } from 'react';
import './RichTextEditor.css';

export interface RichTextEditorProps {
  value: string;
  onChange: (value: string) => void;
  placeholder?: string;
  rows?: number;
  className?: string;
  error?: string;
  id?: string;
}

export const RichTextEditor = ({
  value,
  onChange,
  placeholder = 'Enter description...',
  rows = 6,
  className = '',
  error,
  id,
}: RichTextEditorProps) => {
  const editorRef = useRef<HTMLDivElement>(null);
  const [isFocused, setIsFocused] = useState(false);

  useEffect(() => {
    if (editorRef.current && editorRef.current.innerHTML !== value) {
      editorRef.current.innerHTML = value || '';
    }
  }, [value]);

  const handleInput = () => {
    if (editorRef.current) {
      const html = editorRef.current.innerHTML;
      onChange(html);
    }
  };

  const handlePaste = (e: React.ClipboardEvent) => {
    e.preventDefault();
    const text = e.clipboardData.getData('text/plain');
    document.execCommand('insertText', false, text);
  };

  const execCommand = (command: string, value?: string) => {
    document.execCommand(command, false, value || undefined);
    editorRef.current?.focus();
    handleInput();
  };

  type ToolbarButton = 
    | { command: string; icon: string; label: string; value?: string; separator?: false }
    | { separator: true };

  const toolbarButtons: ToolbarButton[] = [
    { command: 'bold', icon: 'B', label: 'Bold' },
    { command: 'italic', icon: 'I', label: 'Italic' },
    { command: 'underline', icon: 'U', label: 'Underline' },
    { separator: true },
    { command: 'formatBlock', value: '<p>', icon: '¶', label: 'Paragraph' },
    { command: 'formatBlock', value: '<h2>', icon: 'H2', label: 'Heading 2' },
    { command: 'formatBlock', value: '<h3>', icon: 'H3', label: 'Heading 3' },
    { separator: true },
    { command: 'insertUnorderedList', icon: '•', label: 'Bullet List' },
    { command: 'insertOrderedList', icon: '1.', label: 'Numbered List' },
    { separator: true },
    { command: 'justifyLeft', icon: '◄', label: 'Align Left' },
    { command: 'justifyCenter', icon: '◄►', label: 'Align Center' },
    { command: 'justifyRight', icon: '►', label: 'Align Right' },
  ];

  const classes = [
    'rich-text-editor',
    className,
    error && 'rich-text-editor--error',
    isFocused && 'rich-text-editor--focused',
  ]
    .filter(Boolean)
    .join(' ');

  return (
    <div className={classes}>
      <div className="rich-text-editor__toolbar">
        {toolbarButtons.map((btn, index) => {
          if (btn.separator) {
            return <div key={index} className="rich-text-editor__separator" />;
          }
          return (
            <button
              key={index}
              type="button"
              className="rich-text-editor__toolbar-btn"
              onClick={() => execCommand(btn.command, btn.value)}
              title={btn.label}
              aria-label={btn.label}
            >
              {btn.icon}
            </button>
          );
        })}
      </div>
      <div
        ref={editorRef}
        id={id}
        className="rich-text-editor__editor"
        contentEditable
        onInput={handleInput}
        onPaste={handlePaste}
        onFocus={() => setIsFocused(true)}
        onBlur={() => setIsFocused(false)}
        data-placeholder={placeholder}
        style={{ minHeight: `${rows * 1.5}rem` }}
        suppressContentEditableWarning
      />
      {error && <span className="rich-text-editor__error">{error}</span>}
    </div>
  );
};

