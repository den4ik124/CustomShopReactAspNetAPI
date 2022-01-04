import React from "react";
import {useField } from "formik";
import {Form, Label } from "semantic-ui-react";

interface Props{
    placeholder: string;
    rows: number;
    name: string;
    type?: string;
    label?: string;
    // readonly? : boolean
    // value? : string
}

export default function MyTextArea(props: Props){
    const [field, meta] = useField(props.name);
    return (

        <Form.Field>
            <label>{props.label}</label>
            <textarea {...field} {...props} />
            {meta.touched && meta.error ? (
                <Label basic color="red">{meta.error}</Label>
             ) : null }
        </Form.Field> 
    )
}