import { createContext, useContext, useEffect, useState } from "react";

type Theme = "dark" | "light" | "system";
type FontSize = "small" | "medium" | "large";
type ColorTheme = "purple" | "green" | "orange" | "pink" | "cyan";

type ThemeProviderProps = {
  children: React.ReactNode;
  defaultTheme?: Theme;
  storageKey?: string;
};

type ThemeProviderState = {
  theme: Theme;
  setTheme: (theme: Theme) => void;
  fontSize: FontSize;
  setFontSize: (fontSize: FontSize) => void;
  colorTheme: ColorTheme;
  setColorTheme: (colorTheme: ColorTheme) => void;
};

const initialState: ThemeProviderState = {
  theme: "dark",
  setTheme: () => null,
  fontSize: "medium",
  setFontSize: () => null,
  colorTheme: "purple",
  setColorTheme: () => null,
};

const ThemeProviderContext = createContext<ThemeProviderState>(initialState);

export function ThemeProvider({
  children,
  defaultTheme = "dark",
  storageKey = "aimate-theme",
  ...props
}: ThemeProviderProps) {
  const [theme, setThemeState] = useState<Theme>(
    () => (localStorage.getItem(storageKey) as Theme) || defaultTheme
  );
  
  const [fontSize, setFontSizeState] = useState<FontSize>(
    () => (localStorage.getItem("aimate-font-size") as FontSize) || "medium"
  );
  
  const [colorTheme, setColorThemeState] = useState<ColorTheme>(
    () => (localStorage.getItem("aimate-color-theme") as ColorTheme) || "purple"
  );

  useEffect(() => {
    const root = window.document.documentElement;

    root.classList.remove("light", "dark");
    
    // Handle system theme
    if (theme === "system") {
      const systemTheme = window.matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light";
      root.classList.add(systemTheme);
    } else {
      root.classList.add(theme);
    }
  }, [theme]);
  
  useEffect(() => {
    const root = window.document.documentElement;
    
    root.setAttribute("data-font-size", fontSize);
  }, [fontSize]);
  
  useEffect(() => {
    const root = window.document.documentElement;
    
    root.setAttribute("data-color-theme", colorTheme);
  }, [colorTheme]);

  const value = {
    theme,
    setTheme: (theme: Theme) => {
      localStorage.setItem(storageKey, theme);
      setThemeState(theme);
    },
    fontSize,
    setFontSize: (fontSize: FontSize) => {
      localStorage.setItem("aimate-font-size", fontSize);
      setFontSizeState(fontSize);
    },
    colorTheme,
    setColorTheme: (colorTheme: ColorTheme) => {
      localStorage.setItem("aimate-color-theme", colorTheme);
      setColorThemeState(colorTheme);
    },
  };

  return (
    <ThemeProviderContext.Provider {...props} value={value}>
      {children}
    </ThemeProviderContext.Provider>
  );
}

export const useTheme = () => {
  const context = useContext(ThemeProviderContext);

  if (context === undefined)
    throw new Error("useTheme must be used within a ThemeProvider");

  return context;
};
