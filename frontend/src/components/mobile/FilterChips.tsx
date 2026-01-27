import React from 'react';

interface FilterChip {
  id: string | number;
  label: string;
}

interface FilterChipsProps {
  chips: FilterChip[];
  selectedId?: string | number;
  onSelect?: (id: string | number) => void;
}

const FilterChips: React.FC<FilterChipsProps> = ({
  chips,
  selectedId,
  onSelect,
}) => {
  return (
    <div className="flex gap-2 overflow-x-auto pb-2 px-4 scrollbar-hide">
      {chips.map((chip) => {
        const isSelected = selectedId === chip.id;
        return (
          <button
            key={chip.id}
            onClick={() => onSelect?.(chip.id)}
            className={`flex-shrink-0 px-3 py-1.5 rounded-full text-[13px] font-medium transition-colors ${
              isSelected
                ? 'bg-[#6B5B95] text-white'
                : 'bg-white text-[#6B6B6B] border border-[#E5E5EA]'
            }`}
          >
            {chip.label}
          </button>
        );
      })}
    </div>
  );
};

export default FilterChips;
