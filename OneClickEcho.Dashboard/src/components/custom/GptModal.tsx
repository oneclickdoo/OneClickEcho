import { useState } from "react";

import { RiAiGenerate, RiRobot2Line, RiToolsLine, RiThumbUpLine } from "@remixicon/react";

import { useAuth } from "@/context/AuthContext";

import { Dialog, DialogContent, DialogFooter, DialogHeader, DialogTitle, DialogTrigger } from "@/components/tremor/Dialog";
import { Label } from "@/components/tremor/Label";
import { Textarea } from "@/components/tremor/Textarea";
import { Button } from "@/components/tremor/Button";

import { generateMessage, enhanceMessage } from "@/elements/dashboard/singleCampaign/fetchData";

import { useToast } from "@/lib/useToast";

interface IGptModalProps {
    disabled?: boolean;
    type: "Viber" | "SMS";
    campaignId: string;
    message: string | undefined;
    setMessage: (message: string) => void;
}

export const GptModal = (props: IGptModalProps) => {
    const [requestMessage, setRequestMessage] = useState<string>("");
    const [responseMessage, setResponseMessage] = useState<string>("");
    const [isGenerateLoading, setIsGenerateLoading] = useState<boolean>(false);
    const [isEnhanceLoading, setIsEnhanceLoading] = useState<boolean>(false);
    const [open, setOpen] = useState<boolean>(false);

    const { authFetch } = useAuth();

    const { toast } = useToast();

    const getGenerateResponse = async () => {
        setIsGenerateLoading(true);

        try {
            const data = await generateMessage(props.campaignId, requestMessage, authFetch);

            setResponseMessage(data.responseMessage);

            toast({
                variant: "success",
                title: "Success",
                description: `The ${props.type} message has been generated.`,
                duration: 2000
            });
        } catch (e: any) {
            toast({
                variant: "error",
                title: "Error",
                description: e.message,
                duration: 2000
            });
        } finally {
            setIsGenerateLoading(false);
        }
    };

    const getEnhanceResponse = async () => {
        setIsEnhanceLoading(true);

        try {
            const data = await enhanceMessage(props.campaignId, requestMessage, authFetch);

            setResponseMessage(data.responseMessage);

            toast({
                variant: "success",
                title: "Success",
                description: `The ${props.type} message has been enhanced.`,
                duration: 2000
            });
        } catch (e: any) {
            toast({
                variant: "error",
                title: "Error",
                description: e.message,
                duration: 2000
            });
        } finally {
            setIsEnhanceLoading(false);
        }
    };

    return (
        <Dialog
            open={open}
            onOpenChange={() => {
                setRequestMessage(props.message ? props.message : "");
                setResponseMessage("");
                setIsGenerateLoading(false);
                setIsEnhanceLoading(false);
            }}
        >
            <DialogTrigger asChild>
                <Button
                    disabled={props.disabled}
                    type="submit"
                    variant="primary"
                    className="flex gap-x-2 mb-2 border-transparent bg-indigo-600 text-white outline-indigo-500 hover:bg-indigo-500 disabled:bg-indigo-100 disabled:text-gray-400 dark:bg-indigo-500 dark:text-gray-900 dark:outline-indigo-500 dark:hover:bg-indigo-600 disabled:dark:bg-indigo-800 disabled:dark:text-indigo-400"
                    onClick={() => setOpen(true)}
                >
                    <span>AI</span>
                    <RiRobot2Line />
                </Button>
            </DialogTrigger>
            <DialogContent className="sm:max-w-lg" aria-describedby="gpt dialog">
                <DialogHeader>
                    <DialogTitle>You need help from AI?</DialogTitle>
                    <p className="mt-1 text-sm text-gray-400">Generate or enhance a {props.type} meesage using ChatGPT AI tool.</p>
                    <div className="mt-3">
                        <Label>Request</Label>
                        <Textarea
                            value={requestMessage}
                            placeholder="Request message"
                            maxLength={1000}
                            className="h-[100px] mb-6 resize-none"
                            onChange={(e) => setRequestMessage(e.target.value)}
                        />
                        <div className="flex justify-center gap-x-3 mb-2">
                            <Button
                                disabled={!requestMessage || isEnhanceLoading}
                                isLoading={isGenerateLoading}
                                className="flex gap-x-1 w-full sm:w-fit border-transparent bg-green-600 text-white outline-green-500 hover:bg-green-500 disabled:bg-green-100 disabled:text-gray-400 dark:bg-green-500 dark:text-gray-900 dark:outline-green-500 dark:hover:bg-green-600 disabled:dark:bg-green-800 disabled:dark:text-green-400"
                                onClick={getGenerateResponse}
                            >
                                <span>Generate</span>
                                <RiAiGenerate />
                            </Button>
                            <Button
                                disabled={!requestMessage || isGenerateLoading}
                                isLoading={isEnhanceLoading}
                                className="flex gap-x-1 w-full sm:w-fit border-transparent bg-teal-600 text-white outline-teal-500 hover:bg-teal-500 disabled:bg-teal-100 disabled:text-gray-400 dark:bg-teal-500 dark:text-gray-900 dark:outline-teal-500 dark:hover:bg-teal-600 disabled:dark:bg-teal-800 disabled:dark:text-teal-400"
                                onClick={getEnhanceResponse}
                            >
                                <span>Enhance</span>
                                <RiToolsLine />
                            </Button>
                        </div>
                        <Label>Response</Label>
                        <Textarea
                            disabled={!responseMessage}
                            value={responseMessage}
                            placeholder="ChatGPT response"
                            maxLength={1000}
                            className="h-[150px] resize-none"
                        />
                    </div>
                </DialogHeader>
                <DialogFooter className="mt-6 flex justify-end sm:justify-end flex-col gap-2">
                    <Button variant="secondary" className="w-full mt-2 sm:mt-0 sm:w-fit" onClick={() => setOpen(false)}>
                        Go back
                    </Button>
                    <Button
                        disabled={!responseMessage}
                        className="flex gap-x-2 w-full sm:w-fit border-transparent bg-indigo-600 text-white outline-indigo-500 hover:bg-indigo-500 disabled:bg-indigo-100 disabled:text-gray-400 dark:bg-indigo-500 dark:text-gray-900 dark:outline-indigo-500 dark:hover:bg-indigo-600 disabled:dark:bg-indigo-800 disabled:dark:text-indigo-400"
                        onClick={() => {
                            props.setMessage(responseMessage);
                            setOpen(false);
                        }}
                    >
                        <span>Accept</span>
                        <RiThumbUpLine />
                    </Button>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    );
};
