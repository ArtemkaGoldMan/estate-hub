/**
 * Strips HTML tags from a string and returns plain text
 */
export function stripHtml(html: string | null | undefined): string {
  if (!html) return '';

  // Create a temporary DOM element to parse HTML
  const tmp = document.createElement('div');
  tmp.innerHTML = html;
  
  // Get text content (strips all HTML tags)
  return tmp.textContent || tmp.innerText || '';
}





