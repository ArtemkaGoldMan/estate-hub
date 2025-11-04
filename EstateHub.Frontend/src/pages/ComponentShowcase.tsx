import { useState } from 'react';
import {
  Button,
  Dropdown,
  Input,
  Card,
  CardHeader,
  CardBody,
  CardFooter,
  Modal,
  LoadingSpinner,
} from '../shared/ui';
import './ComponentShowcase.css';

export const ComponentShowcase = () => {
  const [modalOpen, setModalOpen] = useState(false);
  const [dropdownValue, setDropdownValue] = useState<string | number>();
  const [inputValue, setInputValue] = useState('');
  const [showLoading, setShowLoading] = useState(false);

  const dropdownOptions = [
    { value: 'option1', label: 'Option 1' },
    { value: 'option2', label: 'Option 2' },
    { value: 'option3', label: 'Option 3', disabled: true },
    { value: 'option4', label: 'Option 4' },
  ];

  const handleLoading = () => {
    setShowLoading(true);
    setTimeout(() => setShowLoading(false), 2000);
  };

  return (
    <div className="component-showcase">
      <header className="showcase-header">
        <h1>EstateHub UI Components</h1>
        <p>Shared component library examples</p>
      </header>

      <div className="showcase-content">
        {/* Button Examples */}
        <section className="component-section">
          <h2>Buttons</h2>
          <div className="component-grid">
            <Button variant="primary">Primary Button</Button>
            <Button variant="secondary">Secondary Button</Button>
            <Button variant="outline">Outline Button</Button>
            <Button variant="danger">Danger Button</Button>
            <Button variant="ghost">Ghost Button</Button>
            <Button size="xs">Extra Small (xs)</Button>
            <Button size="sm">Small (sm)</Button>
            <Button size="md">Medium (md)</Button>
            <Button size="lg">Large (lg)</Button>
            <Button size="xl">Extra Large (xl)</Button>
            <Button isLoading>Loading...</Button>
            <Button fullWidth>Full Width</Button>
            <Button disabled>Disabled</Button>
          </div>
        </section>

        {/* Input Examples */}
        <section className="component-section">
          <h2>Inputs</h2>
          <div className="component-grid">
            <Input
              label="Email"
              type="email"
              placeholder="Enter your email"
              value={inputValue}
              onChange={(e) => setInputValue(e.target.value)}
            />
            <Input
              label="Password"
              type="password"
              placeholder="Enter password"
              helperText="Must be at least 8 characters"
            />
            <Input
              label="With Error"
              error="This field is required"
              value={inputValue}
              onChange={(e) => setInputValue(e.target.value)}
            />
            <Input
              label="Disabled Input"
              value="Disabled value"
              disabled
            />
            <Input
              label="Full Width Input"
              fullWidth
              placeholder="Full width input"
            />
          </div>
        </section>

        {/* Dropdown Examples */}
        <section className="component-section">
          <h2>Dropdowns</h2>
          <div className="component-grid">
            <Dropdown
              label="Select Option"
              options={dropdownOptions}
              value={dropdownValue}
              onChange={(value) => setDropdownValue(value)}
              placeholder="Choose an option..."
            />
            <Dropdown
              label="With Error"
              options={dropdownOptions}
              error="Please select an option"
            />
            <Dropdown
              label="Disabled Dropdown"
              options={dropdownOptions}
              disabled
            />
          </div>
        </section>

        {/* Card Examples */}
        <section className="component-section">
          <h2>Cards</h2>
          <div className="component-grid">
            <Card>
              <CardHeader>Card Title</CardHeader>
              <CardBody>
                <p>This is a basic card with header, body, and footer.</p>
              </CardBody>
              <CardFooter>
                <Button size="sm">Action</Button>
              </CardFooter>
            </Card>
            <Card hoverable>
              <CardBody>
                <h3>Hoverable Card</h3>
                <p>Hover over this card to see the effect.</p>
              </CardBody>
            </Card>
            <Card
              onClick={() => alert('Card clicked!')}
              hoverable
            >
              <CardBody>
                <h3>Clickable Card</h3>
                <p>Click this card to trigger an action.</p>
              </CardBody>
            </Card>
          </div>
        </section>

        {/* Modal Example */}
        <section className="component-section">
          <h2>Modal</h2>
          <div className="component-grid">
            <Button onClick={() => setModalOpen(true)}>Open Modal</Button>
            <Modal
              isOpen={modalOpen}
              onClose={() => setModalOpen(false)}
              title="Example Modal"
              size="medium"
              footer={
                <>
                  <Button variant="outline" onClick={() => setModalOpen(false)}>
                    Cancel
                  </Button>
                  <Button onClick={() => setModalOpen(false)}>Confirm</Button>
                </>
              }
            >
              <p>This is a modal example. You can put any content here.</p>
              <p>Click outside or press Escape to close.</p>
            </Modal>
          </div>
        </section>

        {/* Loading Spinner Example */}
        <section className="component-section">
          <h2>Loading Spinner</h2>
          <div className="component-grid">
            <LoadingSpinner size="small" />
            <LoadingSpinner size="medium" />
            <LoadingSpinner size="large" />
            <LoadingSpinner size="medium" text="Loading..." />
            <Button onClick={handleLoading}>Show Full Screen Loading</Button>
            {showLoading && (
              <LoadingSpinner fullScreen text="Loading data..." />
            )}
          </div>
        </section>
      </div>
    </div>
  );
};

