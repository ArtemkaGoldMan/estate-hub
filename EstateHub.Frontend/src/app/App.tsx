import { Routes, Route } from 'react-router-dom';
import { Home, ComponentShowcase } from '../pages';
import { Navigation } from '../widgets/navigation';
import './index.css';

export const App = () => {
  return (
    <>
      <Navigation />
      <Routes>
        <Route path="/" element={<Home />} />
        <Route path="/components" element={<ComponentShowcase />} />
      </Routes>
    </>
  );
};

