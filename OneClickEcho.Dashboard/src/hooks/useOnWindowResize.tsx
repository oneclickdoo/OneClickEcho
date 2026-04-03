import { useEffect, useRef } from "react";

export const useOnWindowResize = (handler: () => void) => {
    const handlerRef = useRef(handler);

    // Uvek drži najnoviji handler
    useEffect(() => {
        handlerRef.current = handler;
    }, [handler]);

    useEffect(() => {
        const handleResize = () => {
            handlerRef.current();
        };

        handleResize(); // initial call

        window.addEventListener("resize", handleResize);
        return () => window.removeEventListener("resize", handleResize);
    }, []);
};
