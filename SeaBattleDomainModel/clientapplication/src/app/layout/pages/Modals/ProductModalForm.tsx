import { ErrorMessage, Formik, FormikErrors } from 'formik';
import React, { useState } from 'react'
import { Button, Form, Icon, Label, Modal, TextArea } from 'semantic-ui-react'
import MyTextArea from '../../../common/MyTextArea';
import MyTextInput from '../../../common/MyTextInput';
import { useStore } from '../../../stores/store';

interface Props{
    isSubmitting : boolean;
    handleSubmit : (arg1 : React.FormEvent<HTMLFormElement> | undefined) => void // React.FormEvent<HTMLFormElement> | undefined => void;
    applyButtonContent: any
    errors: FormikErrors<{
                          id: string;
                          title: string;
                          price: number;
                          description: string;
                          error: null;
                      }>
  }

function ProductModalForm({handleSubmit, isSubmitting, applyButtonContent, errors} : Props) {
  const [open, setOpen] = useState(false)

  return (
    <Form className="ui form" onSubmit={handleSubmit} autoComplete="off" size="large">
        <MyTextInput name = 'title' placeholder='Product title' type='text'/>
        <MyTextInput name = 'price' placeholder='Product price' type='number'/>
        <MyTextArea name = 'description' rows={5} placeholder='Description' type='text'/>
        <ErrorMessage 
                        name="error"
                        render={() => 
                            <Label 
                                basic 
                                color="red"
                                style ={{marginBottom: 10}}
                                content={errors.error}
                            />
                        }
                    />
        {renderModalButtons(isSubmitting, applyButtonContent)}
    </Form>
  )

  function renderModalButtons(isSubmitting: boolean, buttonContent: any) {
    return <Modal.Actions style={{ marginTop: '20px' }}>
            <Button color='black' onClick={() => setOpen(false)}>
              Cancel
            </Button>
            <Button
              loading={isSubmitting}
              labelPosition='right'
              icon
              positive
              type='submit'>
              <Icon name='checkmark' />
              {buttonContent}
            </Button>
          </Modal.Actions>;
  }
}

export default ProductModalForm