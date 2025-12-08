import { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../../shared/context/AuthContext';
import { useToast } from '../../../shared/context/ToastContext';
import { userApi, type GetUserResponse } from '../../../shared/api/auth';

export const useProfilePage = () => {
  const navigate = useNavigate();
  const { isAuthenticated, user, logout, setUser } = useAuth();
  const { showSuccess, showError } = useToast();
  const [profile, setProfile] = useState<GetUserResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');
  const [displayName, setDisplayName] = useState('');
  const [phoneNumber, setPhoneNumber] = useState('');
  const [country, setCountry] = useState('');
  const [city, setCity] = useState('');
  const [address, setAddress] = useState('');
  const [postalCode, setPostalCode] = useState('');
  const [companyName, setCompanyName] = useState('');
  const [website, setWebsite] = useState('');
  const [avatarFile, setAvatarFile] = useState<File | null>(null);
  const [avatarPreview, setAvatarPreview] = useState<string | null>(null);
  const [showDeleteModal, setShowDeleteModal] = useState(false);
  const [deleting, setDeleting] = useState(false);

  useEffect(() => {
    if (!user) {
      return;
    }

    const loadProfile = async () => {
      try {
        setLoading(true);
        setError('');
        const userProfile = await userApi.getUser(user.id);
        setProfile(userProfile);
        setDisplayName(userProfile.displayName);
        setPhoneNumber(userProfile.phoneNumber || '');
        setCountry(userProfile.country || '');
        setCity(userProfile.city || '');
        setAddress(userProfile.address || '');
        setPostalCode(userProfile.postalCode || '');
        setCompanyName(userProfile.companyName || '');
        setWebsite(userProfile.website || '');
        if (userProfile.avatar) {
          setAvatarPreview(userProfile.avatar);
        }
      } catch (err) {
        const errorMessage = err instanceof Error ? err.message : 'Failed to load profile';
        setError(errorMessage);
        showError(errorMessage);
      } finally {
        setLoading(false);
      }
    };

    loadProfile();
  }, [isAuthenticated, user, navigate, showError]);

  const handleAvatarChange = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      if (!file.type.startsWith('image/')) {
        const errorMessage = 'Please select an image file';
        setError(errorMessage);
        showError(errorMessage);
        return;
      }

      if (file.size > 2 * 1024 * 1024) {
        const errorMessage = 'Image size must be less than 2MB';
        setError(errorMessage);
        showError(errorMessage);
        return;
      }

      setAvatarFile(file);
      setError('');

      const reader = new FileReader();
      reader.onloadend = () => {
        setAvatarPreview(reader.result as string);
      };
      reader.readAsDataURL(file);
    }
  }, [showError]);

  const handleSave = useCallback(async () => {
    if (!user || !profile) return;

    try {
      setSaving(true);
      setError('');

      // Always send all fields to prevent clearing unchanged fields
      await userApi.updateUser(user.id, {
        displayName: displayName,
        avatar: avatarFile || undefined,
        phoneNumber: phoneNumber,
        country: country,
        city: city,
        address: address,
        postalCode: postalCode,
        companyName: companyName,
        website: website,
      });

      const updatedProfile = await userApi.getUser(user.id);
      setProfile(updatedProfile);
      setDisplayName(updatedProfile.displayName);
      setPhoneNumber(updatedProfile.phoneNumber || '');
      setCountry(updatedProfile.country || '');
      setCity(updatedProfile.city || '');
      setAddress(updatedProfile.address || '');
      setPostalCode(updatedProfile.postalCode || '');
      setCompanyName(updatedProfile.companyName || '');
      setWebsite(updatedProfile.website || '');
      setAvatarFile(null);

      if (user && displayName !== user.displayName) {
        setUser({
          ...user,
          displayName: updatedProfile.displayName,
          avatar: updatedProfile.avatar,
        });
      }

      showSuccess('Profile updated successfully!');
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to update profile';
      setError(errorMessage);
      showError(errorMessage);
    } finally {
      setSaving(false);
    }
  }, [user, profile, displayName, phoneNumber, country, city, address, postalCode, companyName, website, avatarFile, setUser, showSuccess, showError]);

  const handleDeleteAccount = useCallback(async () => {
    if (!user) return;

    try {
      setDeleting(true);
      setError('');

      await userApi.deleteUser(user.id);
      await logout();
      navigate('/listings', { replace: true });
      showSuccess('Your account has been deleted.');
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to delete account';
      setError(errorMessage);
      showError(errorMessage);
      setDeleting(false);
    }
  }, [user, logout, navigate, showSuccess, showError]);

  const handleCancel = useCallback(() => {
    if (!profile) return;
    setDisplayName(profile.displayName);
    setPhoneNumber(profile.phoneNumber || '');
    setCountry(profile.country || '');
    setCity(profile.city || '');
    setAddress(profile.address || '');
    setPostalCode(profile.postalCode || '');
    setCompanyName(profile.companyName || '');
    setWebsite(profile.website || '');
    setAvatarFile(null);
    setAvatarPreview(profile.avatar || null);
    setError('');
  }, [profile]);

  const handleCancelAvatar = useCallback(() => {
    if (!profile) return;
    setAvatarFile(null);
    setAvatarPreview(profile.avatar || null);
  }, [profile]);

  const hasChanges = !!profile && (
    displayName !== profile.displayName ||
    phoneNumber !== (profile.phoneNumber || '') ||
    country !== (profile.country || '') ||
    city !== (profile.city || '') ||
    address !== (profile.address || '') ||
    postalCode !== (profile.postalCode || '') ||
    companyName !== (profile.companyName || '') ||
    website !== (profile.website || '') ||
    avatarFile !== null
  );

  return {
    profile,
    loading,
    saving,
    error,
    deleting,
    isAuthenticated,
    user,
    displayName,
    phoneNumber,
    country,
    city,
    address,
    postalCode,
    companyName,
    website,
    avatarPreview,
    avatarFile,
    showDeleteModal,
    hasChanges,
    setDisplayName,
    setPhoneNumber,
    setCountry,
    setCity,
    setAddress,
    setPostalCode,
    setCompanyName,
    setWebsite,
    handleAvatarChange,
    handleCancelAvatar,
    handleSave,
    handleCancel,
    handleDeleteAccount,
    setShowDeleteModal,
    navigate,
  };
};

