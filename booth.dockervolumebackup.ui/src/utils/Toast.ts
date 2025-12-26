import { atom, useAtom } from 'jotai';

export interface Toast {
    type: 'success' | 'error' | 'info';
    title: string;
    description: string;
    actions?: ToastAction[];
}

export interface ToastAction {
    label: string;
    action: () => void;
}

export interface UseToastResult {
    showToast: (toast: Toast) => void;
}

export const toastAtom = atom<Toast[]>([]);

export const useToast = (): UseToastResult => {

    const [toasts, setToasts] = useAtom(toastAtom);

    const showToast = (toast: Toast): void => {
        setToasts([...toasts, toast]);
    }

    return { showToast }
}