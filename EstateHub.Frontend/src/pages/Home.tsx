import { Link } from 'react-router-dom';
import { Button, Card, CardBody } from '../shared/ui';
import './Home.css';

export const Home = () => {
  return (
    <div className="home-page">
      <header className="home-header">
        <h1>Welcome to EstateHub</h1>
        <p>Your real estate platform</p>
      </header>

      <div className="home-content">
        <Card>
          <CardBody>
            <h2>Get Started</h2>
            <p>Explore the EstateHub platform and start browsing properties.</p>
            <div className="home-actions">
              <Link to="/components">
                <Button variant="primary">View UI Components</Button>
              </Link>
            </div>
          </CardBody>
        </Card>
      </div>
    </div>
  );
};

