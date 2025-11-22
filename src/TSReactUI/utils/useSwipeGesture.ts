import { useRef, useEffect } from 'react';

interface SwipeGestureOptions {
  onSwipeLeft?: () => void;
  onSwipeRight?: () => void;
  threshold?: number;
}

export function useSwipeGesture(options: SwipeGestureOptions) {
  const { onSwipeLeft, onSwipeRight, threshold = 50 } = options;
  const touchStartX = useRef<number>(0);
  const touchEndX = useRef<number>(0);
  const ref = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const element = ref.current;
    if (!element) return;

    const handleTouchStart = (e: TouchEvent) => {
      touchStartX.current = e.touches[0].clientX;
    };

    const handleTouchMove = (e: TouchEvent) => {
      touchEndX.current = e.touches[0].clientX;
    };

    const handleTouchEnd = () => {
      const diff = touchStartX.current - touchEndX.current;
      
      if (Math.abs(diff) > threshold) {
        if (diff > 0 && onSwipeLeft) {
          // Swiped left
          onSwipeLeft();
        } else if (diff < 0 && onSwipeRight) {
          // Swiped right
          onSwipeRight();
        }
      }
    };

    element.addEventListener('touchstart', handleTouchStart);
    element.addEventListener('touchmove', handleTouchMove);
    element.addEventListener('touchend', handleTouchEnd);

    return () => {
      element.removeEventListener('touchstart', handleTouchStart);
      element.removeEventListener('touchmove', handleTouchMove);
      element.removeEventListener('touchend', handleTouchEnd);
    };
  }, [onSwipeLeft, onSwipeRight, threshold]);

  return ref;
}
