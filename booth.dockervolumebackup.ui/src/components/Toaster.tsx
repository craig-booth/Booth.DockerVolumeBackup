import { Flex, Text } from '@radix-ui/themes';
import { Cross2Icon, CheckCircledIcon, CrossCircledIcon, InfoCircledIcon } from '@radix-ui/react-icons'
import * as RadixUiToast from '@radix-ui/react-toast';
import { useAtom } from 'jotai';
import { Toast, toastAtom } from '@/utils/Toast'


export const Toaster = () => {

    return (
        <RadixUiToast.Provider duration={50000}>
            <RadixUiToast.Viewport className="ToastViewport" />

            <ToastMessages />

        </RadixUiToast.Provider>

    )
}

const ToastMessages = () => {

    const [toasts] = useAtom(toastAtom);

    return (
        <>
            {toasts.map((toast: Toast, index: number) => (
                <ToastMessage toast={toast} key={index} />
            ))}

        </>

    )

}


const ToastMessage = ({ toast }: { toast: Toast }) => {

    return (
        <RadixUiToast.Root className="ToastRoot">
            {toast.type === 'error' && <CrossCircledIcon className="ToastIcon" color="red" />}
            {toast.type === 'info' && <InfoCircledIcon className="ToastIcon" color="blue" />}
            {toast.type === 'success' && <CheckCircledIcon className="ToastIcon" color="green" />}
            <Text className="ToastTitle" size="3" weight="bold">{toast.title}</Text>
            <RadixUiToast.Close className="ToastClose" >
                <Cross2Icon />
            </RadixUiToast.Close>
            <Text className="ToastDescription" size="2">{toast.description}</Text>
            <Flex className="ToastActions" direction="row" gap="3">
                {
                    toast.actions?.map((action, index) => (
                        <RadixUiToast.Action className="ToastAction" altText={action.label} key={index} onClick={() => action.action()}>{action.label}</RadixUiToast.Action>
                    ))
                }
            </Flex>
        </RadixUiToast.Root>
    )
}