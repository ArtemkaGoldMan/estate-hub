import clsx from 'clsx';
import './Pagination.css';

interface PaginationProps {
  currentPage: number;
  totalItems: number;
  pageSize: number;
  onPageChange: (page: number) => void;
  disabled?: boolean;
}

const getVisiblePages = (current: number, total: number) => {
  const visible: number[] = [];
  const maxVisible = 5;

  let start = Math.max(current - Math.floor(maxVisible / 2), 1);
  let end = start + maxVisible - 1;

  if (end > total) {
    end = total;
    start = Math.max(end - maxVisible + 1, 1);
  }

  for (let page = start; page <= end; page += 1) {
    visible.push(page);
  }

  return visible;
};

export const Pagination = ({
  currentPage,
  totalItems,
  pageSize,
  onPageChange,
  disabled = false,
}: PaginationProps) => {
  const totalPages = Math.max(Math.ceil(totalItems / pageSize), 1);
  const pages = getVisiblePages(currentPage, totalPages);

  const handleChange = (page: number) => {
    if (!disabled && page >= 1 && page <= totalPages && page !== currentPage) {
      onPageChange(page);
    }
  };

  return (
    <nav className="pagination" aria-label="Listings pagination">
      <button
        type="button"
        className="pagination__control"
        onClick={() => handleChange(currentPage - 1)}
        disabled={disabled || currentPage === 1}
      >
        Prev
      </button>

      {pages[0] > 1 && (
        <>
          <button
            type="button"
            className="pagination__page"
            onClick={() => handleChange(1)}
            disabled={disabled}
          >
            1
          </button>
          {pages[0] > 2 && <span className="pagination__ellipsis">…</span>}
        </>
      )}

      {pages.map((page) => (
        <button
          key={page}
          type="button"
          className={clsx(
            'pagination__page',
            page === currentPage && 'pagination__page--active'
          )}
          onClick={() => handleChange(page)}
          aria-current={page === currentPage ? 'page' : undefined}
          disabled={disabled}
        >
          {page}
        </button>
      ))}

      {pages[pages.length - 1] < totalPages && (
        <>
          {pages[pages.length - 1] < totalPages - 1 && (
            <span className="pagination__ellipsis">…</span>
          )}
          <button
            type="button"
            className="pagination__page"
            onClick={() => handleChange(totalPages)}
            disabled={disabled}
          >
            {totalPages}
          </button>
        </>
      )}

      <button
        type="button"
        className="pagination__control"
        onClick={() => handleChange(currentPage + 1)}
        disabled={disabled || currentPage === totalPages}
      >
        Next
      </button>
    </nav>
  );
};


