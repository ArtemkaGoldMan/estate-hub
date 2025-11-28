import { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../shared/context/AuthContext';
import { userApi, type GetUserResponse } from '../../shared/api/auth';
import { Button, Input, LoadingSpinner, Modal } from '../../shared';
import './ProfilePage.css';

export const ProfilePage = () => {
  const navigate = useNavigate();
  const { isAuthenticated, user, logout, setUser } = useAuth();
  const [profile, setProfile] = useState<GetUserResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');
  const [displayName, setDisplayName] = useState('');
  const [avatarFile, setAvatarFile] = useState<File | null>(null);
  const [avatarPreview, setAvatarPreview] = useState<string | null>(null);
  const [showDeleteModal, setShowDeleteModal] = useState(false);
  const [deleting, setDeleting] = useState(false);

  // Note: Route protection is now handled by ProtectedRoute component
  useEffect(() => {
    if (!user) {
      return;
    }

    // Load user profile
    const loadProfile = async () => {
      try {
        setLoading(true);
        setError('');
        const userProfile = await userApi.getUser(user.id);
        setProfile(userProfile);
        setDisplayName(userProfile.displayName);
        if (userProfile.avatar) {
          setAvatarPreview(userProfile.avatar);
        }
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Failed to load profile');
      } finally {
        setLoading(false);
      }
    };

    loadProfile();
  }, [isAuthenticated, user, navigate]);

  const handleAvatarChange = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      // Validate file type
      if (!file.type.startsWith('image/')) {
        setError('Please select an image file');
        return;
      }

      // Validate file size (2MB)
      if (file.size > 2 * 1024 * 1024) {
        setError('Image size must be less than 2MB');
        return;
      }

      setAvatarFile(file);
      setError('');

      // Create preview
      const reader = new FileReader();
      reader.onloadend = () => {
        setAvatarPreview(reader.result as string);
      };
      reader.readAsDataURL(file);
    }
  }, []);

  const handleSave = useCallback(async () => {
    if (!user || !profile) return;

    try {
      setSaving(true);
      setError('');

      await userApi.updateUser(user.id, {
        displayName: displayName !== profile.displayName ? displayName : undefined,
        avatar: avatarFile || undefined,
      });

      // Reload profile to get updated data
      const updatedProfile = await userApi.getUser(user.id);
      setProfile(updatedProfile);
      setAvatarFile(null);

      // Update auth context if display name changed
      if (user && displayName !== user.displayName) {
        setUser({
          ...user,
          displayName: updatedProfile.displayName,
          avatar: updatedProfile.avatar,
        });
      }

      // Show success message (you can add a toast notification here)
      alert('Profile updated successfully!');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to update profile');
    } finally {
      setSaving(false);
    }
  }, [user, profile, displayName, avatarFile, setUser]);

  const handleDeleteAccount = useCallback(async () => {
    if (!user) return;

    try {
      setDeleting(true);
      setError('');

      await userApi.deleteUser(user.id);
      await logout();
      navigate('/listings', { replace: true });
      alert('Your account has been deleted.');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to delete account');
      setDeleting(false);
    }
  }, [user, logout, navigate]);

  if (!isAuthenticated || !user) {
    return null;
  }

  if (loading) {
    return (
      <div className="profile-page">
        <div className="profile-page__loading">
          <LoadingSpinner text="Loading profile..." />
        </div>
      </div>
    );
  }

  if (!profile) {
    return (
      <div className="profile-page">
        <div className="profile-page__error">
          <h2>Failed to load profile</h2>
          <p>{error || 'Please try again later.'}</p>
          <Button onClick={() => navigate('/dashboard')}>Back to Dashboard</Button>
        </div>
      </div>
    );
  }

  return (
    <div className="profile-page">
      <div className="profile-page__header">
        <h1>Profile & Settings</h1>
        <Button variant="ghost" onClick={() => navigate('/dashboard')}>
          ← Back to Dashboard
        </Button>
      </div>

      {error && (
        <div className="profile-page__error-banner">
          <p>{error}</p>
        </div>
      )}

      <div className="profile-page__content">
        <div className="profile-page__section">
          <h2>Profile Information</h2>
          
          <div className="profile-page__avatar-section">
            <div className="profile-page__avatar-preview">
              {avatarPreview ? (
                <img src={avatarPreview} alt="Avatar preview" />
              ) : (
                <div className="profile-page__avatar-placeholder">
                  {displayName.charAt(0).toUpperCase()}
                </div>
              )}
            </div>
            <div className="profile-page__avatar-controls">
              <label htmlFor="avatar-upload" className="profile-page__avatar-label">
                <span className="profile-page__avatar-button">
                  <Button variant="outline">
                    Change Avatar
                  </Button>
                </span>
                <input
                  id="avatar-upload"
                  type="file"
                  accept="image/jpeg,image/jpg,image/png"
                  onChange={handleAvatarChange}
                  style={{ display: 'none' }}
                />
              </label>
              {avatarFile && (
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => {
                    setAvatarFile(null);
                    setAvatarPreview(profile.avatar || null);
                  }}
                >
                  Cancel
                </Button>
              )}
            </div>
            <p className="profile-page__avatar-hint">
              JPG, PNG up to 2MB. Recommended: 32x32px minimum.
            </p>
          </div>

          <div className="profile-page__field">
            <label htmlFor="email">Email</label>
            <Input
              id="email"
              type="email"
              value={profile.email}
              disabled
              readOnly
            />
            <p className="profile-page__field-hint">Email cannot be changed</p>
          </div>

          <div className="profile-page__field">
            <label htmlFor="displayName">Display Name</label>
            <Input
              id="displayName"
              type="text"
              value={displayName}
              onChange={(e) => setDisplayName(e.target.value)}
              placeholder="Enter your display name"
              maxLength={50}
            />
          </div>

          <div className="profile-page__field">
            <label>User ID</label>
            <Input
              type="text"
              value={profile.id}
              disabled
              readOnly
            />
          </div>
        </div>

        <div className="profile-page__section">
          <h2>Account Actions</h2>
          
          <div className="profile-page__actions">
            <Button
              variant="primary"
              onClick={handleSave}
              disabled={saving || (displayName === profile.displayName && !avatarFile)}
              isLoading={saving}
            >
              {saving ? 'Saving...' : 'Save Changes'}
            </Button>
            
            <Button
              variant="outline"
              onClick={() => {
                setDisplayName(profile.displayName);
                setAvatarFile(null);
                setAvatarPreview(profile.avatar || null);
                setError('');
              }}
              disabled={saving}
            >
              Cancel
            </Button>
          </div>
        </div>

        <div className="profile-page__section profile-page__section--danger">
          <h2>Danger Zone</h2>
          <p className="profile-page__danger-text">
            Once you delete your account, there is no going back. Please be certain.
          </p>
          <Button
            variant="danger"
            onClick={() => setShowDeleteModal(true)}
            className="profile-page__delete-button"
          >
            Delete Account
          </Button>
        </div>
      </div>

      <Modal
        isOpen={showDeleteModal}
        onClose={() => setShowDeleteModal(false)}
        title="Delete Account"
      >
        <div className="profile-page__delete-modal">
          <p>Are you sure you want to delete your account? This action cannot be undone.</p>
          <div className="profile-page__delete-modal-actions">
            <Button
              variant="outline"
              onClick={() => setShowDeleteModal(false)}
              disabled={deleting}
            >
              Cancel
            </Button>
            <Button
              variant="danger"
              onClick={handleDeleteAccount}
              disabled={deleting}
              isLoading={deleting}
            >
              {deleting ? 'Deleting...' : 'Delete Account'}
            </Button>
          </div>
        </div>
      </Modal>
    </div>
  );
};

