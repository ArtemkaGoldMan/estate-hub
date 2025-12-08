import { useState, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../../shared/context/AuthContext';
import { useMyListingsQuery, useLikedListingsQuery, useArchivedListingsQuery } from '../../../entities/listing';

const PAGE_SIZE = 12;

export type TabType = 'my-listings' | 'liked-listings' | 'archived-listings';

export const useDashboardPage = () => {
  const navigate = useNavigate();
  const { user } = useAuth();
  const [activeTab, setActiveTab] = useState<TabType>('my-listings');
  const [myListingsPage, setMyListingsPage] = useState(1);
  const [likedListingsPage, setLikedListingsPage] = useState(1);
  const [archivedListingsPage, setArchivedListingsPage] = useState(1);

  // Only query the active tab to reduce unnecessary network requests
  const { data: myListingsData, loading: myListingsLoading, error: myListingsError, refetch: refetchMyListings } = 
    useMyListingsQuery(myListingsPage, PAGE_SIZE, {
      enablePolling: true,
      pollInterval: 10000,
      skip: activeTab !== 'my-listings',
    });
  
  const { data: likedListingsData, loading: likedListingsLoading, error: likedListingsError, refetch: refetchLikedListings } = 
    useLikedListingsQuery(likedListingsPage, PAGE_SIZE, {
      skip: activeTab !== 'liked-listings',
    });
  
  const { data: archivedListingsData, loading: archivedListingsLoading, error: archivedListingsError, refetch: refetchArchivedListings } = 
    useArchivedListingsQuery(archivedListingsPage, PAGE_SIZE, {
      skip: activeTab !== 'archived-listings',
    });

  const handleStatusChange = useCallback(() => {
    if (activeTab === 'my-listings') {
      refetchMyListings();
    } else if (activeTab === 'liked-listings') {
      refetchLikedListings();
    } else if (activeTab === 'archived-listings') {
      refetchArchivedListings();
    }
  }, [activeTab, refetchMyListings, refetchLikedListings, refetchArchivedListings]);

  const handleCreateListing = useCallback(() => {
    navigate('/listings/new');
  }, [navigate]);

  const activeData = activeTab === 'my-listings' 
    ? myListingsData 
    : activeTab === 'liked-listings' 
    ? likedListingsData 
    : archivedListingsData;
  const activeLoading = activeTab === 'my-listings' 
    ? myListingsLoading 
    : activeTab === 'liked-listings' 
    ? likedListingsLoading 
    : archivedListingsLoading;
  const activeError = activeTab === 'my-listings' 
    ? myListingsError 
    : activeTab === 'liked-listings' 
    ? likedListingsError 
    : archivedListingsError;
  const activePage = activeTab === 'my-listings' 
    ? myListingsPage 
    : activeTab === 'liked-listings' 
    ? likedListingsPage 
    : archivedListingsPage;
  const setActivePage = activeTab === 'my-listings' 
    ? setMyListingsPage 
    : activeTab === 'liked-listings' 
    ? setLikedListingsPage 
    : setArchivedListingsPage;

  const listings = activeData ?? { items: [], total: 0 };

  return {
    user,
    activeTab,
    activeData,
    activeLoading,
    activeError,
    activePage,
    listings,
    myListingsData,
    likedListingsData,
    archivedListingsData,
    PAGE_SIZE,
    setActiveTab,
    setActivePage,
    handleStatusChange,
    handleCreateListing,
    navigate,
  };
};


