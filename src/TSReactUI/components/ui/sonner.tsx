"use client";

import { useTheme } from "../ThemeProvider";
import { Toaster as Sonner, ToasterProps } from "sonner@2.0.3";

const Toaster = ({ ...props }: ToasterProps) => {
  const { theme = "system" } = useTheme();

  return (
    <Sonner
      theme={theme as ToasterProps["theme"]}
      className="toaster group"
      position="top-center"
      toastOptions={{
        classNames: {
          toast: 'group toast group-[.toaster]:bg-background group-[.toaster]:text-foreground group-[.toaster]:border-border group-[.toaster]:shadow-lg',
          description: 'group-[.toast]:text-muted-foreground',
          actionButton: 'group-[.toast]:bg-primary group-[.toast]:text-primary-foreground',
          cancelButton: 'group-[.toast]:bg-muted group-[.toast]:text-muted-foreground',
          success: 'group-[.toaster]:!bg-green-500 group-[.toaster]:!text-white group-[.toaster]:!border-green-600',
          error: 'group-[.toaster]:!bg-red-500 group-[.toaster]:!text-white group-[.toaster]:!border-red-600',
        },
      }}
      {...props}
    />
  );
};

export { Toaster };
