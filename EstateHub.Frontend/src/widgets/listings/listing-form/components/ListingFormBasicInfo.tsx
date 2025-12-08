import { Input, RichTextEditor } from '../../../../shared';
import type { ListingCategory, PropertyType, ListingCondition } from '../../../../entities/listing';

interface ListingFormBasicInfoProps {
  category: ListingCategory;
  propertyType: PropertyType;
  title: string;
  description: string;
  condition: ListingCondition;
  errors: Record<string, string>;
  onCategoryChange: (value: ListingCategory) => void;
  onPropertyTypeChange: (value: PropertyType) => void;
  onTitleChange: (value: string) => void;
  onDescriptionChange: (value: string) => void;
  onConditionChange: (value: ListingCondition) => void;
}

export const ListingFormBasicInfo = ({
  category,
  propertyType,
  title,
  description,
  condition,
  errors,
  onCategoryChange,
  onPropertyTypeChange,
  onTitleChange,
  onDescriptionChange,
  onConditionChange,
}: ListingFormBasicInfoProps) => {
  return (
    <div className="listing-form__section">
      <h2>Basic Information</h2>

      <div className="listing-form__field">
        <label htmlFor="category">Category *</label>
        <select
          id="category"
          value={category}
          onChange={(e) => onCategoryChange(e.target.value as ListingCategory)}
          className={errors.category ? 'error' : ''}
        >
          <option value="SALE">For Sale</option>
          <option value="RENT">For Rent</option>
        </select>
        {errors.category && <span className="error-message">{errors.category}</span>}
      </div>

      <div className="listing-form__field">
        <label htmlFor="propertyType">Property Type *</label>
        <select
          id="propertyType"
          value={propertyType}
          onChange={(e) => onPropertyTypeChange(e.target.value as PropertyType)}
          className={errors.propertyType ? 'error' : ''}
        >
          <option value="APARTMENT">Apartment</option>
          <option value="HOUSE">House</option>
          <option value="STUDIO">Studio</option>
          <option value="ROOM">Room</option>
          <option value="OTHER">Other</option>
        </select>
        {errors.propertyType && <span className="error-message">{errors.propertyType}</span>}
      </div>

      <div className="listing-form__field">
        <label htmlFor="title">Title *</label>
        <Input
          id="title"
          type="text"
          value={title}
          onChange={(e) => onTitleChange(e.target.value)}
          placeholder="e.g., Beautiful 2-bedroom apartment in city center"
          maxLength={200}
          error={errors.title}
        />
      </div>

      <div className="listing-form__field">
        <label htmlFor="description">Description *</label>
        <RichTextEditor
          id="description"
          value={description}
          onChange={onDescriptionChange}
          placeholder="Describe your property... You can use formatting like bold, italic, paragraphs, and lists."
          rows={6}
          error={errors.description}
        />
      </div>

      <div className="listing-form__field">
        <label htmlFor="condition">Condition *</label>
        <select
          id="condition"
          value={condition}
          onChange={(e) => onConditionChange(e.target.value as ListingCondition)}
        >
          <option value="NEW">New</option>
          <option value="GOOD">Good</option>
          <option value="NEEDS_RENOVATION">Needs Renovation</option>
        </select>
      </div>
    </div>
  );
};



