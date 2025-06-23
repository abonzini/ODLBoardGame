import { useState, useEffect, useCallback } from 'react';

export function useImageCache() {
  const [loadedImages, setLoadedImages] = useState(new Set());
  const [loadingImages, setLoadingImages] = useState(new Set());
  const [failedImages, setFailedImages] = useState(new Set());

  // Load a single image
  const loadImage = useCallback((src) => {
    return new Promise((resolve, reject) => {
      const img = new Image();
      
      img.onload = () => {
        setLoadedImages(prev => new Set([...prev, src]));
        setLoadingImages(prev => {
          const newSet = new Set(prev);
          newSet.delete(src);
          return newSet;
        });
        resolve(img);
      };
      
      img.onerror = () => {
        setFailedImages(prev => new Set([...prev, src]));
        setLoadingImages(prev => {
          const newSet = new Set(prev);
          newSet.delete(src);
          return newSet;
        });
        reject(new Error(`Failed to load image: ${src}`));
      };
      
      img.src = src;
      setLoadingImages(prev => new Set([...prev, src]));
    });
  }, []);

  // Preload multiple images
  const preloadImages = useCallback(async (imageUrls) => {
    const promises = imageUrls.map(url => loadImage(url));
    try {
      await Promise.all(promises);
      return true;
    } catch (error) {
      console.error('Some images failed to preload:', error);
      return false;
    }
  }, [loadImage]);

  // Check if an image is loaded
  const isImageLoaded = useCallback((src) => {
    return loadedImages.has(src);
  }, [loadedImages]);

  // Check if an image is loading
  const isImageLoading = useCallback((src) => {
    return loadingImages.has(src);
  }, [loadingImages]);

  // Check if an image failed to load
  const isImageFailed = useCallback((src) => {
    return failedImages.has(src);
  }, [failedImages]);

  // Clear all cached images (useful for testing or memory management)
  const clearCache = useCallback(() => {
    setLoadedImages(new Set());
    setLoadingImages(new Set());
    setFailedImages(new Set());
  }, []);

  return {
    loadImage,
    preloadImages,
    isImageLoaded,
    isImageLoading,
    isImageFailed,
    clearCache,
    loadedImages,
    loadingImages,
    failedImages
  };
} 