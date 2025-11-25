// hooks/useIntersectionObserver.js
import { useState, useEffect, useRef } from "react";

export const useIntersectionObserver = (options = {}) => {
  const [isIntersecting, setIsIntersecting] = useState(false);
  const [hasAnimated, setHasAnimated] = useState(false);
  const ref = useRef(null);

  useEffect(() => {
    const element = ref.current;
    if (!element) return;

    const observer = new IntersectionObserver(
      ([entry]) => {
        setIsIntersecting(entry.isIntersecting);

        // Optional: Only animate once
        if (entry.isIntersecting && !hasAnimated) {
          setHasAnimated(true);
        }
      },
      {
        threshold: 0.1,
        rootMargin: "-50px",
        ...options,
      }
    );

    observer.observe(element);

    return () => observer.disconnect();
  }, [options, hasAnimated]);

  return [ref, isIntersecting, hasAnimated];
};
