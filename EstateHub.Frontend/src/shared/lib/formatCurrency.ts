export const formatCurrency = (
  value?: number | null,
  currency: string = 'PLN'
) => {
  if (value === undefined || value === null || Number.isNaN(value)) {
    return '—';
  }

  return new Intl.NumberFormat('pl-PL', {
    style: 'currency',
    currency,
    maximumFractionDigits: 0,
  }).format(value);
};


